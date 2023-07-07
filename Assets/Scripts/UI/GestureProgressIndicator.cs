using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace DFKI.NMY
{
    public class GestureProgressIndicator : ProgressIndicatorLoadingBar
    {
        [SerializeField] private GestureSequencePlayer _player;
        private void LateUpdate()
        {
            _player = _player==null? GestureSequencePlayer.instance:_player;
            
            if (_player.isPlayingLeft){
                Progress =_player.PlayAllSequences ? _player.normalizedProgressTotalLeft : _player.normalizedProgressLeft;
            }
            if (_player.isPlayingRight) {
               Progress = _player.PlayAllSequences ? _player.normalizedProgressTotalRight : _player.normalizedProgressRight;
            }

        }
    }
}
