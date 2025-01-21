using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Formats.Alembic;
using UnityEngine.Formats.Alembic.Importer;
using TMPro;

namespace DFKI.NMY
{
    public class HandFadenAnimator : MonoBehaviour
    {
        public AlembicStreamPlayer[] alembicStreamPlayers;
        
        public Animator animator_left;
        public Animator animator_right;

        public Button playBtn;
        public Button pauseBtn;


        private bool _init;
        private bool _isReadyForPlayback;
        private bool _isPlaying;

        public float currentTime = 0f;


        // Start is called before the first frame update
        void Start()
        {
            if (alembicStreamPlayers.Length != 0 && animator_left != null && animator_right != null)
            {
                _init = true;
                //PrepareBVH();
                Debug.Log("HandFadenAnimator init successful.");
            }

            playBtn.onClick.AddListener(TogglePlayback);
            pauseBtn.onClick.AddListener(Reset);
        }

        // Update is called once per frame
        void Update()
        {
            if (!_init)
                return;
            if (_isPlaying)
            {
                for (int i = 0; i < alembicStreamPlayers.Length; i++)
                {
                    alembicStreamPlayers[i].CurrentTime = currentTime;
                }
                currentTime += Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_isPlaying)
                {
                    Stop();
                }
                else
                    Play();
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Reset();
            }
        }

        public void TogglePlayback()
        {
            if (_isPlaying)
            {
                Stop();
            }
            else
            {
                Play();
            }
        }

        [ContextMenu("Play")]
        public void Play()
        {
            if (!_init)
                return;

            animator_left.speed = 1f;
            animator_right.speed = 1f;

            animator_left.SetBool("isPlaying", true);
            animator_right.SetBool("isPlaying", true);

            _isPlaying = true;

            Debug.Log("HandFadenAnimator start playing.");
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            if (!_init)
                return;

            animator_left.speed = 0f;
            animator_right.speed = 0f;

            _isPlaying = false;

            Debug.Log("HandFadenAnimator paused playing.");
        }

        [ContextMenu("Reset")]
        public void Reset()
        {
            if (!_init)
                return;

            currentTime = 0f;

            animator_left.SetBool("isPlaying", false);
            animator_right.SetBool("isPlaying", false);

            int baseLayerLeft = animator_left.GetLayerIndex("Base Layer");
            AnimatorClipInfo[] currentClipInfoLeft = animator_left.GetCurrentAnimatorClipInfo(baseLayerLeft);
            Debug.Log("currentClipInfoLeft[0].clip.name: " + currentClipInfoLeft[0].clip.name);
            animator_left.Play(currentClipInfoLeft[0].clip.name, baseLayerLeft, 0f);
            animator_left.speed = 1f;

            int baseLayerRight = animator_right.GetLayerIndex("Base Layer");
            AnimatorClipInfo[] currentClipInfoRight = animator_right.GetCurrentAnimatorClipInfo(baseLayerRight);
            Debug.Log("currentClipInfoRight[0].clip.name: " + currentClipInfoRight[0].clip.name);
            animator_right.Play(currentClipInfoRight[0].clip.name, baseLayerRight, 0f);
            animator_right.speed = 1f;

            if (_isPlaying)
            {
                animator_left.SetBool("isPlaying", true);
                animator_right.SetBool("isPlaying", true);
            }

            for (int i = 0; i < alembicStreamPlayers.Length; i++)
            {
                alembicStreamPlayers[i].CurrentTime = 0f;
            }

            Debug.Log("HandFadenAnimator reset to beginning.");
        }
    }
}
