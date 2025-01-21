namespace DFKI.NMY
{
    public interface IKnotbAR
    {
        TrainingPhase Phase { get; set; }
        bool AffectTimer { get; set; }
        
        virtual void UpdateTrainingPhase(TrainingPhase phase) => Phase = phase;
        
        
    }
}
