using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System.Text;
using UnityEngine.UI;

namespace DFKI {

    /*
     * The class models the BVH module for a single skeleton hierarchy.
     * Note: for two hands there must be two gesture modules
     */
    public class GestureModule {

        private bool debug = false;
        public void SetDebug(bool flag = true) { debug = flag; }

        /*
         * NOTE: Siblings are ordered alphabetically ...
         */
        public readonly string[] hierarchy_names = { "elbow", "wrist", "index", "middle", "pinky", "ring", "thumb" };

        // Start indices of joints per BVH frame.
        // Note: this potentially should go into an extra class which defines
        // a general hierarchy.
        public const int elbow = 0;    // 6 values: 3 position, 3 rotation
        public const int wrist = 6;    // 3 values: rotation
        public const int index_prox = 9;    // ...
        public const int index_b = 12;
        public const int index_c = 15;
        public const int middle_prox = 18;
        public const int middle_b = 21;
        public const int middle_c = 24;
        public const int pinky_prox = 27;
        public const int pinky_b = 30;
        public const int pinky_c = 33;
        public const int ring_prox = 36;
        public const int ring_b = 39;
        public const int ring_c = 42;
        public const int thumb_prox = 45;
        public const int thumb_a = 48;   // ...
        public const int thumb_b = 51;   // 3 values: rotation
        public const int param_count = 54;   // last index < count

        public int GetParameterCount() { return param_count; }

        private float[][] bvhData;
        private bool[] bvhValid;
        private string bvhFilePath = "none";


        public void SetWristWeights(float x, float y, float z)
        {
            weightMask[6] = z;
            weightMask[7] = x;
            weightMask[8] = y;
        }
        
        
        public void SetElbowWeights(float x, float y, float z)
        {
            weightMask[3] = z;
            weightMask[4] = x;
            weightMask[5] = y;
        }

        
        private float[] weightMask = new float[54]
        {
            0f,   // 0 elbow POS X
            0f,   // 1 elbow POS Y
            0f,   // 2 elbow POS Z
            0f,   // 3 elbow ROT Z
            0f,   // 4 elbow ROT X
            0f,   // 5 elbow ROT Y
            
            5f,   // 6 wrist ROT Z
            0.5f,  // 7 wrist ROT X
            0.5f,   // 8 wrist ROT Y
            
            0f,   // index proximal ROT Z
            0.5f, // index proximal ROT X
            1f,   // index proximal ROT Y
            0f,   // index b ROT Z
            0.5f, // index b ROT X
            1f,   // index b ROT Y
            0f,   // index c ROT Z
            0.5f, // index c ROT X
            1f,   // index c ROT Y
            0f,   // middle proximal ROT Z
            0.5f, // middle proximal ROT X
            1f,   // middle proximal ROT Y
            0f,   // middle b ROT Z
            0.5f, // middle b ROT X
            1f,   // middle b ROT Y
            0f,   // middle c ROT Z
            0.5f, // middle c ROT X
            1f,   // middle c ROT Y
            0f,   // pinky proximal ROT Z
            0.5f, // pinky proximal ROT X
            1f,   // pinky proximal ROT Y
            0f,   // pinky b ROT Z
            0.5f, // pinky b ROT X
            1f,   // pinky b ROT Y
            0f,   // pinky c ROT Z
            0.5f, // pinky c ROT X
            1f,   // pinky c ROT Y
            0f,   // ring proximal ROT Z
            0.5f, // ring proximal ROT X
            1f,   // ring proximal ROT Y
            0f,   // ring b ROT Z
            0.5f, // ring b ROT X
            1f,   // ring b ROT Y
            0f,   // ring c ROT Z
            0.5f, // ring c ROT X
            1f,   // ring c ROT Y
            0f,   // thumb proximal ROT Z
            0.5f, // thumb proximal ROT X
            1f,   // thumb proximal ROT Y
            0f,   // thumb a ROT Z
            0.5f, // thumb a ROT X
            1f,   // thumb a ROT Y
            0f,   // thumb b ROT Z                
            0.5f, // thumb b ROT X
            1f    // thumb b ROT Y
        };

        /*
         * General terminology:
         * A POSE is a single configuration of a skeleton and does not have a special meaning.
         * A GESTURE is a pose of a skeleton with a specific meaning.
         * Both poses and gestures can be static or dynamic. Dynamic poses are just movements,
         * while dynamic gestures are movements that carry meaning.
         * 
         * The whole motion sequence will be segmented into sub-sequences.
         * Each features four different frame indices:
         * 1. Start pose index
         * 2. End pose index
         * 3. Sequence start frame index
         * 4. Sequence end frame index
         * 
         * Use Start and End pose indices in case you want to visualize static depictions of poses.
         * The end pose is the target pose e.g. when counting (US) the first start pose is a fist and
         * the first end pose is a fist with extended index finger. For counting the first goal would
         * be to count to one. Hence, the end pose is what the user should assume for successful counting.
         * Note also that the end pose of a specific action is the start pose of the subsequent action.
         * 
         * Use start and end sequence frame indices for dynamic visualizations. In general, the sequence
         * start and end indices will not equal the start and end pose indices. However, it should
         * be always true that:
         * start pose index <= sequence start frame index <= sequence end frame index <= end pose index
         *
         * Be sure to check the valid flags of a frame prior to using it. Frames tagged invalid are those
         * that were captured while the LEAP failed to track the hand. Valid flags of left and right hands
         * will differ in general!
         * 
         */
        private List<int> sequenceStart;
        private List<int> sequenceEnd;
        private List<int> sequencePoseIndices;
        private StringBuilder sb = new StringBuilder();
        private bool _printDebug;
        private bool firstFrameRun = true;
        private bool secondFrameRun = false;
        private int firstFrameIndex;

        //public float[] currentDigitDeviation { get; private set; }
        public float currentMeanDeviation { get; private set; }

        /*
         * Function returns the file path where the data was read from
         */
        public string GetFilePath() { return bvhFilePath; }

        /*
         * Function returns the total number of frames of BVH data
         */
        public int GetNumberOfFrames() { return bvhData.GetLength(0); }

        /*
         * Function returns a specific frame
         */
        public float[] GetFrame(int index) { return bvhData[index]; }

        /*
         * Function returns whether frame is valid or not
         */
        public bool GetFrameIsValid(int index) { return bvhValid[index]; }

        /*
         * Function returns valid flags for all frames
         */
        public ReadOnlyCollection<bool> GetFramesValid() { return new ReadOnlyCollection<bool>(bvhValid); }

        /*
         * Function returns the index of a start pose with index from [0, GetNumberOfSequences()]
         * !!! DO NOT CONFUSE WITH SEQUENCE START INDEX !!!
         */
        public int GetStartPoseIndex(int index) { return sequencePoseIndices[index]; }

        /*
         * Function returns the index of an end pose with index from [0, GetNumberOfSequences()]
         * !!! DO NOT CONFUSE WITH SEQUENCE END INDEX !!!
         */
        public int GetEndPoseIndex(int index) { return sequencePoseIndices[index + 1]; }

        /*
         * Function returns length of sequence
         */
        public int GetSequenceLength(int index) { return sequenceEnd[index] - sequenceStart[index]; }

        /*
         * Function returns start frame index of a sub-sequence
         * !!! DO NOT CONFUSE WITH START POSE INDEX !!!
         */
        public int GetSequenceStartFrameIndex(int index) { return sequenceStart[index]; }

        /*
         * Function returns end frame index of a sub-sequence
         * !!! DO NOT CONFUSE WITH END POSE INDEX !!!
         */
        public int GetSequenceEndFrameIndex(int index) { return sequenceEnd[index]; }

        /*
         * Function returns the number of sequences
         */
        public int GetNumberOfSequences() { return sequenceStart.Count; }

        /*
         * Function compares an external BVH frame to an internal BVH frame given by index
         * 
         * index: the index into the internal BVH data
         * frame: an external BVH frame
         * threshold: similarity threshold
         * mask: a boolean mask whether to ignore (false) a specific frame value
         * 
         * If no mask is given all deviations are taken into account
         * 
         */
        public bool SimilarPose(int index, float[] frame, float threshold, bool[] mask = null) {
            if (mask != null)
            Assert.AreEqual(param_count, mask.Length, "Mask does not match number of parameters. (158)");
            float[] currentDeviations = CalculateFrameDeviation(index, frame);
            if (currentDeviations==null) return false;
            
            //currentDigitDeviation = CalculateDigitDeviation(currentDeviations);

            // old approach comparing single value deviations of the current frame
            //for (int i = 0; i < param_count; i++) {
            //    if (mask == null || mask[i]) {
            //        if (threshold < Mathf.Abs(currentDeviations[i]))
            //            return false;
            //    }
            //}

            // new approach comparing the overall deviation of the current frame
            currentMeanDeviation = CalculateMeanDeviation(currentDeviations, mask);
            if (threshold < currentMeanDeviation)
                return false;

            return true;
        }

        public void PrintDebug()
        {
            _printDebug = true;
        }

        float CalculateMeanDeviation(float[] frame, bool[] mask = null)
        {
            float total = 0.0f;
            int valCount = 0;
                    
            for (int i = 0; i < param_count; i++)
            {
                if (mask == null || mask[i])
                {
                    total += Mathf.Abs(frame[i] * weightMask[i]);
                    valCount++;
                }
            }
            
            var result = total / valCount;
            //Debug.Log(" Mean deviation: " + result);
            return result;
        }

        public float GetMaximumDifferenceFromCurrentPose(int index, float[] frame, bool[] mask = null) {
            float maximum = 0.0f;
            if (mask != null)
                Assert.AreEqual(param_count, mask.Length, "Mask does not match number of parameters. (158)");
            float[] floats = CalculateFrameDeviation(index, frame);
            if (floats==null) return maximum;
            for (int i = 0; i < param_count; i++) {
                if (mask == null || mask[i]) {
                    maximum = Mathf.Abs(floats[i]) > maximum ? Mathf.Abs(floats[i]) : maximum;
                }
            }
            return maximum;
        }

        /*
         * Function pre-processes the given file. Should be called in a start function of MonoBehavior
         */
        public void BVHPreprocessing(string filePath) {

            ReadBVHFile(filePath);
            MarkFrames();

            List<int> sequences = SegmentFramesIntoSequences();
            Assert.AreNotEqual(sequences.Count, 0, "Sequence count is 0. (178)");

            //Debug.Log("Sequence items: " + sequences.Count);

            sequenceStart = new List<int>();
            sequenceEnd = new List<int>();
            sequencePoseIndices = new List<int>();
            
            
            // HOTFIX

            if (sequences.Count % 2 == 1)
            {
                sequences.RemoveAt(sequences.Count-1);
            }

            for (int i = 1; i < sequences.Count - 1; i++) {
                if (i % 2 == 1)
                    sequenceStart.Add(sequences[i]);
                else
                    sequenceEnd.Add(sequences[i]);
            }
            if (debug) {
                for (int i=0; i< sequenceStart.Count; i++) {
                    Debug.Log("Sequence[" + i + "]: " + sequenceStart[i] + " --> " + sequenceEnd[i]);
                }
            }

            sequencePoseIndices.Add((int)((sequences[0] + sequences[1]) / 2));
            for (int i = 2; i < sequences.Count - 1; i+=2) {
                sequencePoseIndices.Add((int)((sequences[i] + sequences[i + 1]) / 2));
            }
            sequencePoseIndices.Add((int)((sequences[^2] + sequences[^1]) / 2));

            /*for (int i = 0; i < sequencePoseIndices.Count; i++)
                Debug.Log("Item " + i + " = " + sequencePoseIndices[i]);
            */

            Assert.AreEqual(sequenceStart.Count, sequenceEnd.Count, "Start and End sequences do not match elements. (203)");
            Assert.AreEqual(sequenceStart.Count + 2, sequencePoseIndices.Count, "Sequences and poses do not match: " + (sequenceStart.Count + 2) + ", " + sequencePoseIndices.Count + ". (204)");
        }

        /*
         * Function parses a single BVH data line
         * 
         * input: the data line to parse
         * 
         */
        private float[] ParseFloats(string input) {
            // Cleanup consecutive spaces
            string line = Regex.Replace(input, @"\s{1,}", " ");
            line = line.Trim();
            string[] tokens = line.Split(' ');
            List<float> floats = new();
            foreach (string token in tokens) {
                if (float.TryParse(token, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float value)) {
                    floats.Add(value);
                }
            }
            return floats.ToArray();
        }

        /*
         * Function reads the complete data portion of a BVH file
         * 
         * filePath: the file path to the data
         * 
         */
        private void ReadBVHFile(string filePath) {
            List<float[]> matrix = new();
            using (StreamReader reader = new(filePath)) {
                string line;
                // skip to end of header
                while ((line = reader.ReadLine()) != null) {
                    if (line.StartsWith("Frame Time:")) {
                        break; // Stop parsing the header
                    }
                }
                // read the BVH data
                while ((line = reader.ReadLine()) != null) {
                    float[] values = ParseFloats(line);
                    matrix.Add(values);
                }
            }
            // save BVH file path
            bvhFilePath = filePath;
            bvhData = matrix.ToArray();
            if (debug) {
                Debug.Log(bvhData.GetLength(0) + " lines read from " + filePath);
            }
        }

        /*
         * Function check if two frames are EXACTLY the same
         * 
         */
        private bool SameFrames(int idx1, int idx2) {
            for (int i = 0; i < bvhData[idx1].GetLength(0); i++) {
                if (bvhData[idx1][i] != bvhData[idx2][i]) {
                    return false;
                }
            }
            return true;
        }

        /*
         * Function creates and populates array with valid flags for each frame.
         * In case two frames are identical the UltraLEAP did not generate data.
         */
        private void MarkFrames() {
            bvhValid = new bool[bvhData.GetLength(0)];
            bvhValid[0] = true;
            for (int idx = 1; idx < bvhData.GetLength(0); idx++) {
                bvhValid[idx] = !SameFrames(idx, idx - 1);
            }
        }

        /*
         * Function computes per component deviation for a stored BVH frame with a sampled frame
         * 
         * frameIdx: the frame index to compare to (i.e., stored in class)
         * frame: a frame coming from the outside (i.e., current user frame)
         * 
         */
        public float[] CalculateFrameDeviation(int frameIdx, float[] frame) {
            if (frame == null)
                return null;
            Assert.AreEqual(bvhData[frameIdx].GetLength(0), frame.Length, "Frames do not match in size: " + bvhData[frameIdx].GetLength(0) + " != " + frame.Length + ". (290)");
            float[] deviation = new float[bvhData[frameIdx].GetLength(0)];
            //if (_printDebug)
            //{
            //    if (firstFrameRun == true)
            //    {
            //        firstFrameRun = false;
            //        firstFrameIndex = frameIdx;
            //        secondFrameRun = true;
            //    }
            //    if (secondFrameRun == true && frameIdx == firstFrameIndex)
            //    {
            //        sb.Clear();
            //        for (int i = 0; i < bvhData[frameIdx].GetLength(0); i++)
            //        {
            //            deviation[i] = bvhData[frameIdx][i] - frame[i];
            //            sb.AppendLine(frame[i] + ";" + bvhData[frameIdx][i]);
            //        }
            //        Debug.Log(sb.ToString());
            //        _printDebug = false;
            //    }
            //    else
            //    {
            //        for (int i = 0; i < bvhData[frameIdx].GetLength(0); i++)
            //        {
            //            deviation[i] = bvhData[frameIdx][i] - frame[i];
            //        }
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < bvhData[frameIdx].GetLength(0); i++)
            //    {
            //        deviation[i] = bvhData[frameIdx][i] - frame[i];
            //    }
            //}

            for (int i = 0; i < bvhData[frameIdx].GetLength(0); i++)
            {
                deviation[i] = bvhData[frameIdx][i] - frame[i];
            }
            return deviation;
        }

        /*
        public float[] CalculateDigitDeviation(float[] frameDeviation)
        {
            if (frameDeviation == null)
                return null;
            float[] digitDeviation = new float[5];
            digitDeviation[0] = Mathf.Abs(frameDeviation[thumb_prox] * weightMask[thumb_prox] + frameDeviation[thumb_prox + 1] * weightMask[thumb_prox + 1]  + frameDeviation[thumb_prox + 2] * weightMask[thumb_prox + 2])
                + Mathf.Abs(frameDeviation[thumb_a] * weightMask[thumb_a] + frameDeviation[thumb_a + 1] * weightMask[thumb_a + 1] + frameDeviation[thumb_a + 2] * weightMask[thumb_a + 2])
                + Mathf.Abs(frameDeviation[thumb_b] * weightMask[thumb_b] + frameDeviation[thumb_b + 1] * weightMask[thumb_b + 1] + frameDeviation[thumb_b + 2] * weightMask[thumb_b + 2]);

            digitDeviation[1] = Mathf.Abs(frameDeviation[index_prox] * weightMask[index_prox] + frameDeviation[index_prox + 1] * weightMask[index_prox + 1] + frameDeviation[index_prox + 2] * weightMask[index_prox + 2])
                + Mathf.Abs(frameDeviation[index_b] * weightMask[index_b] + frameDeviation[index_b + 1] * weightMask[index_b + 1] + frameDeviation[index_b + 2] * weightMask[index_b + 2])
                + Mathf.Abs(frameDeviation[index_c] * weightMask[index_c] + frameDeviation[index_c + 1] * weightMask[index_c + 1] + frameDeviation[index_c + 2] * weightMask[index_c + 2]);

            digitDeviation[2] = Mathf.Abs(frameDeviation[middle_prox] * weightMask[middle_prox] + frameDeviation[middle_prox + 1] * weightMask[middle_prox + 1] + frameDeviation[middle_prox + 2] * weightMask[middle_prox + 2])
                + Mathf.Abs(frameDeviation[middle_b] * weightMask[middle_b] + frameDeviation[middle_b + 1] * weightMask[middle_b + 1] + frameDeviation[middle_b + 2] * weightMask[middle_b + 2])
                + Mathf.Abs(frameDeviation[middle_c] * weightMask[middle_c] + frameDeviation[middle_c + 1] * weightMask[middle_c + 1] + frameDeviation[middle_c + 2] * weightMask[middle_c + 2]);

            digitDeviation[3] = Mathf.Abs(frameDeviation[ring_prox] * weightMask[ring_prox] + frameDeviation[ring_prox + 1] * weightMask[ring_prox + 1] + frameDeviation[ring_prox + 2] * weightMask[ring_prox + 2])
                + Mathf.Abs(frameDeviation[ring_b] * weightMask[ring_b] + frameDeviation[ring_b + 1] * weightMask[ring_b + 1] + frameDeviation[ring_b + 2] * weightMask[ring_b + 2])
                + Mathf.Abs(frameDeviation[ring_c] * weightMask[ring_c] + frameDeviation[ring_c + 1] * weightMask[ring_c + 1] + frameDeviation[ring_c + 2] * weightMask[ring_c + 2]);

            digitDeviation[4] = Mathf.Abs(frameDeviation[pinky_prox] * weightMask[pinky_prox] + frameDeviation[pinky_prox + 1] * weightMask[pinky_prox + 1] + frameDeviation[pinky_prox + 2] * weightMask[pinky_prox + 2])
                + Mathf.Abs(frameDeviation[pinky_b] * weightMask[pinky_b] + frameDeviation[pinky_b + 1] * weightMask[pinky_b + 1] + frameDeviation[pinky_b + 2] * weightMask[pinky_b + 2])
                + Mathf.Abs(frameDeviation[pinky_c] * weightMask[pinky_c] + frameDeviation[pinky_c + 1] * weightMask[pinky_c + 1] + frameDeviation[pinky_c + 2] * weightMask[pinky_c + 2]);

            return digitDeviation;
        }*/

        /*
         * Function computes the maximum delta between two frames
         * 
         * frame1: first BVH frame (reference)
         * frame2: second BVH frame (variable)
         * 
         * Function is deprecated since it just cares for the maximum error of the whole hierarchy
         * 
         */
        private float CalculateMaxAbsDeltaOfTwoFrames(float[] frame1, float[] frame2) {
            float maximum = -float.MaxValue;
            // Starting from 3 i.e. ignore root's xyz position
            for (int i = 9; i < frame1.GetLength(0); i++) {
                maximum = Mathf.Max(maximum, Mathf.Abs(frame1[i] - frame2[i]));
                // Debug.Log("frame1.GetLength(0): "+ frame1.GetLength(0));
                //Debug.Log("maximum: " + maximum);
            }
            return maximum;
        }

        /*
         * Function segments the BVH frame sequence into sub-sequences
         * 
         * windowSize: Size of averaging window (15 seems to be a good default value)
         */
        private List<int> SegmentFramesIntoSequences(int windowSize = 15) {

            // ---------------------------------------------------------------------------------
            // Calculate Deltas
            // ---------------------------------------------------------------------------------
            float[] Delta = new float[bvhData.GetLength(0) - 1];
            for (int i = 0; i < bvhData.GetLength(0) - 1; i++) {
                Delta[i] = CalculateMaxAbsDeltaOfTwoFrames(bvhData[i + 1], bvhData[i]);
            }

            // ---------------------------------------------------------------------------------
            // Apply average window
            // ---------------------------------------------------------------------------------
            int avgSize = bvhData.GetLength(0) - windowSize + 1;
            float[] avg = new float[avgSize];
            for (int i = 0; i < avgSize; i++) {
                float sum = 0;
                for (int j = i; j < i + windowSize - 1; j++) {
                    sum += Delta[j];
                }
                avg[i] = sum / windowSize;
            }

            // ---------------------------------------------------------------------------------
            // Apply hard threshold
            // ---------------------------------------------------------------------------------
            /*
            float sum_delta=0;
            for(int i=0; i<Delta.GetLength(0);i++)
            {
                sum_delta += Delta[i];
            }
            float segThreshold = sum_delta/Delta.GetLength(0);

            */

            float sum_avg = 0;
            for (int i = 0; i < avg.GetLength(0); i++) {
                sum_avg += avg[i];
            }
            float segThreshold = sum_avg / avg.GetLength(0);
            if (debug) {
                Debug.Log("BVH segmentation threshold: " + segThreshold);
            }

            float[] avg_thr = new float[avgSize];
            for (int i = 0; i < avgSize; i++) {
                avg_thr[i] = (avg[i] > segThreshold ? segThreshold : 0);
            }

            // -------------------------------------------------------------------------
            // Get representative poses and start/end sequence indices
            // -------------------------------------------------------------------------
            float[] delta_avg_thr = new float[avgSize - 1];
            List<int> sequences = new() { 0 };
            for (int i = 0; i < avgSize - 1; i++) {
                delta_avg_thr[i] = avg_thr[i + 1] - avg_thr[i];
            }
            for (int i = 0; i < avgSize - 1; i++) {
                if (delta_avg_thr[i] != 0) {
                    sequences.Add(i + windowSize / 2);
                }
            }
            sequences.Add(avgSize - 1);
            return sequences;
        }
    }
}