using System.Threading.Tasks;
using SoundpadConnector.Response;

namespace SoundpadConnector {
    public partial class SoundpadConnector {
        public async Task<NoContentResponse> PlaySound(int index) {
            return await Send<NoContentResponse>($"DoPlaySound({index})");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="renderLine">Play on speaker</param>
        /// <param name="captureLine">Play on microphone</param>
        /// <returns></returns>
        public async Task<NoContentResponse> PlaySound(int index, bool renderLine, bool captureLine) {
            return await Send<NoContentResponse>($"DoPlaySound({index}, {renderLine}, {captureLine})");
        }

        public async Task<NoContentResponse> PlayPreviousSound() {
            return await Send<NoContentResponse>($"DoPlayPreviousSound()");
        }

        public async Task<NoContentResponse> PlayNextSound() {
            return await Send<NoContentResponse>($"DoPlayNextSound()");
        }

        public async Task<NoContentResponse> StopSound() {
            return await Send<NoContentResponse>($"DoStopSound()");
        }

        public async Task<NoContentResponse> TogglePause() {
            return await Send<NoContentResponse>($"DoTogglePause()");
        }

        /// <summary>
        /// Jumps fowards or backwards. Use negative value to jump backwards.
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> Jump(int milliseconds) {
            return await Send<NoContentResponse>($"DoJumpMs({milliseconds})");
        }

        public async Task<NoContentResponse> StartRecording() {
            return await Send<NoContentResponse>($"DoStartRecording()");
        }

        public async Task<NoContentResponse> StopRecording() {
            return await Send<NoContentResponse>($"DoStopRecording()");
        }

        public async Task<NoContentResponse> Search(string searchTerm) {
            return await Send<NoContentResponse>($"DoSearch(\"{searchTerm}\")");
        }

        public async Task<NoContentResponse> ResetAndHideSearch() {
            return await Send<NoContentResponse>($"DoResetSearch()");
        }

        public async Task<NoContentResponse> SelectPreviousHit() {
            return await Send<NoContentResponse>($"DoSelectPreviousHit()");
        }

        public async Task<NoContentResponse> SelectNextHit() {
            return await Send<NoContentResponse>($"DoSelectNextHit()");
        }

        public async Task<NoContentResponse> DoSelectIndex(int index) {
            return await Send<NoContentResponse>($"DoSelectIndex({index})");
        }

        /// <summary>
        /// Scrolls up or down. Use negative value to scroll up;
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> Scroll(int rows) {
            return await Send<NoContentResponse>($"DoScrollBy({rows})");
        }

        public async Task<NoContentResponse> ScrollTo(int index) {
            return await Send<NoContentResponse>($"DoScrollTo({index})");
        }

        public async Task<NumberResponse> GetSoundFileCount() {
            return await Send<NumberResponse>($"GetSoundFileCount()");
        }

        /// <summary>
        /// Returns MS
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetPlaybackPosition() {
            return await Send<NumberResponse>($"GetPlaybackPositionInMs()");
        }

        /// <summary>
        /// Returns MS
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetPlaybackDuration() {
            return await Send<NumberResponse>($"GetPlaybackDurationInMs()");
        }

        /// <summary>
        /// Returns MS
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetRecordingPosition() {
            return await Send<NumberResponse>($"GetRecordingPositionInMs()");
        }

        /// <summary>
        /// Returns MS
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetRecordingPeak() {
            return await Send<NumberResponse>($"GetRecordingPeak()");
        }

        public async Task<SoundlistResponse> GetSoundlist() {
            return await Send<SoundlistResponse>("GetSoundlist()");
        }

        public async Task<SoundlistResponse> GetSoundlist(int fromIndex) {
            return await Send<SoundlistResponse>($"GetSoundlist({fromIndex})");
        }

        public async Task<SoundlistResponse> GetSoundlist(int fromIndex, int toIndex) {
            return await Send<SoundlistResponse>($"GetSoundlist({fromIndex}, {toIndex})");
        }

        public async Task<TextResponse> GetMainFrameTitleText() {
            return await Send<TextResponse>("GetMainFrameTitleText()");
        }

        public async Task<TextResponse> GetStatusBarText() {
            return await Send<TextResponse>("GetStatusBarText()");
        }

        public async Task<PlayStatusResponse> GetPlayStatus() {
            return await Send<PlayStatusResponse>("GetPlayStatus()");
        }

        public async Task<NoContentResponse> AddSound(string url) {
            return await Send<NoContentResponse>($"DoAddSound(\"{url}\")");
        }

        public async Task<NoContentResponse> AddSound(string url, int index) {
            return await Send<NoContentResponse>($"DoAddSound(\"{url}\", {index})");
        }

        public async Task<NoContentResponse> RemoveSelectedEntries(bool removeFromDisk = false) {
            return await Send<NoContentResponse>($"DoRemoveSelectedEntries({removeFromDisk})");
        }

        public async Task<NoContentResponse> Undo() {
            return await Send<NoContentResponse>($"DoUndo()");
        }

        public async Task<NoContentResponse> Redo() {
            return await Send<NoContentResponse>($"DoRedo()");
        }

        public async Task<NoContentResponse> SaveSoundlist() {
            return await Send<NoContentResponse>($"DoSaveSoundlist()");
        }

        public async Task<NumberResponse> GetVolume() {
            return await Send<NumberResponse>($"GetVolume()");
        }

        public async Task<BooleanResponse> IsMuted() {
            return await Send<BooleanResponse>($"IsMuted()");
        }

        public async Task<NoContentResponse> SetVolume(int volume) {
            return await Send<NoContentResponse>($"SetVolume({volume})");
        }

        public async Task<NoContentResponse> ToggleMute() {
            return await Send<NoContentResponse>($"DoToggleMute()");
        }

        public async Task<NoContentResponse> IsAlive() {
            return await Send<NoContentResponse>($"IsAlive()");
        }

        public async Task<TextResponse> GetVersion() {
            return await Send<TextResponse>("GetRemoteControlVersion()");
        }
    }
}