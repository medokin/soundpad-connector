using System.Threading.Tasks;
using SoundpadConnector.CustomApi;
using SoundpadConnector.Response;

namespace SoundpadConnector
{
    public partial class Soundpad
    {
        /// <summary>
        ///     Plays sound
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> PlaySound(int index)
        {
            return await Send<NoContentResponse>($"DoPlaySound({index})");
        }

        /// <summary>
        ///     Plays sound by index on specific line
        /// </summary>
        /// <param name="index"></param>
        /// <param name="renderLine">Play on speaker</param>
        /// <param name="captureLine">Play on microphone</param>
        /// <returns></returns>
        public async Task<NoContentResponse> PlaySound(int index, bool renderLine, bool captureLine)
        {
            return await Send<NoContentResponse>($"DoPlaySound({index}, {renderLine}, {captureLine})");
        }

        /// <summary>
        ///     Plays previous sound
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> PlayPreviousSound()
        {
            return await Send<NoContentResponse>($"DoPlayPreviousSound()");
        }

        /// <summary>
        ///     Plays next sound
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> PlayNextSound()
        {
            return await Send<NoContentResponse>($"DoPlayNextSound()");
        }

        /// <summary>
        ///     Stopps playback
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> StopSound()
        {
            return await Send<NoContentResponse>($"DoStopSound()");
        }

        /// <summary>
        ///     Toggles pause
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> TogglePause()
        {
            return await Send<NoContentResponse>($"DoTogglePause()");
        }

        /// <summary>
        /// Jumps fowards or backwards. Use negative value to jump backwards.
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> Jump(int milliseconds)
        {
            return await Send<NoContentResponse>($"DoJumpMs({milliseconds})");
        }

        /// <summary>
        ///     Starts recording
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> StartRecording()
        {
            return await Send<NoContentResponse>($"DoStartRecording()");
        }

        /// <summary>
        ///     Stops recording
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> StopRecording()
        {
            return await Send<NoContentResponse>($"DoStopRecording()");
        }

        /// <summary>
        ///     Executes a search in Soundpad
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> Search(string searchTerm)
        {
            return await Send<NoContentResponse>($"DoSearch(\"{searchTerm}\")");
        }

        /// <summary>
        ///     Resets and hides search in Soundpad
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> ResetSearch()
        {
            return await Send<NoContentResponse>($"DoResetSearch()");
        }

        /// <summary>
        ///     Selects previous search hit in Soundpad
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> SelectPreviousHit()
        {
            return await Send<NoContentResponse>($"DoSelectPreviousHit()");
        }

        /// <summary>
        ///     Selects next search hit in Soundpad
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> SelectNextHit()
        {
            return await Send<NoContentResponse>($"DoSelectNextHit()");
        }

        /// <summary>
        ///     Selects a sound in Soundpad
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> SelectIndex(int index)
        {
            return await Send<NoContentResponse>($"DoSelectIndex({index})");
        }

        /// <summary>
        ///     Scrolls up or down in Soundpad. Use negative value to scroll up;
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> Scroll(int rows)
        {
            return await Send<NoContentResponse>($"DoScrollBy({rows})");
        }

        /// <summary>
        ///     Scrolls to a sound in Soundpad.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> ScrollTo(int index)
        {
            return await Send<NoContentResponse>($"DoScrollTo({index})");
        }

        /// <summary>
        ///     Gets sum of sounds
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetSoundFileCount()
        {
            return await Send<NumberResponse>($"GetSoundFileCount()");
        }

        /// <summary>
        ///     Gets current playback position in milliseconds
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetPlaybackPosition()
        {
            return await Send<NumberResponse>($"GetPlaybackPositionInMs()");
        }

        /// <summary>
        ///     Gets current playback duration in milliseconds
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetPlaybackDuration()
        {
            return await Send<NumberResponse>($"GetPlaybackDurationInMs()");
        }

        /// <summary>
        ///     Gets current recording position in milliseconds
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetRecordingPosition()
        {
            return await Send<NumberResponse>($"GetRecordingPositionInMs()");
        }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetRecordingPeak()
        {
            return await Send<NumberResponse>($"GetRecordingPeak()");
        }

        /// <summary>
        ///     Gets current soundlist
        /// </summary>
        /// <returns></returns>
        public async Task<SoundlistResponse> GetSoundlist()
        {
            return await Send<SoundlistResponse>("GetSoundlist()");
        }

        /// <summary>
        ///     Gets current soundlist
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <returns></returns>
        public async Task<SoundlistResponse> GetSoundlist(int fromIndex)
        {
            return await Send<SoundlistResponse>($"GetSoundlist({fromIndex})");
        }

        /// <summary>
        ///     Gets current soundlist
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        /// <returns></returns>
        public async Task<SoundlistResponse> GetSoundlist(int fromIndex, int toIndex)
        {
            return await Send<SoundlistResponse>($"GetSoundlist({fromIndex}, {toIndex})");
        }

        /// <summary>
        ///     TODO: Comment
        /// </summary>
        /// <returns></returns>
        public async Task<TextResponse> GetMainFrameTitleText()
        {
            return await Send<TextResponse>("GetMainFrameTitleText()");
        }

        /// <summary>
        ///     TODO: Comment
        /// </summary>
        /// <returns></returns>
        public async Task<TextResponse> GetStatusBarText()
        {
            return await Send<TextResponse>("GetStatusBarText()");
        }

        /// <summary>
        ///     Gets Playback status
        /// </summary>
        /// <returns></returns>
        public async Task<PlayStatusResponse> GetPlayStatus()
        {
            return await Send<PlayStatusResponse>("GetPlayStatus()");
        }

        /// <summary>
        ///     Adds a sound to soudlist
        /// </summary>
        /// <param name="path">Absolute file path</param>
        /// <returns></returns>
        public async Task<NoContentResponse> AddSound(string path)
        {
            return await Send<NoContentResponse>($"DoAddSound(\"{path}\")");
        }

        /// <summary>
        ///     Adds a sound to soudlist at given index
        /// </summary>
        /// <param name="path">Absolute file path</param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> AddSound(string path, int index)
        {
            return await Send<NoContentResponse>($"DoAddSound(\"{path}\", {index})");
        }

        /// <summary>
        ///     Removes selected sounds from soundlist.
        /// </summary>
        /// <param name="removeFromDisk">If true, Soundpads asks for confirmation</param>
        /// <returns></returns>
        public async Task<NoContentResponse> RemoveSelectedEntries(bool removeFromDisk = false)
        {
            return await Send<NoContentResponse>($"DoRemoveSelectedEntries({removeFromDisk})");
        }

        /// <summary>
        ///     Undo
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> Undo()
        {
            return await Send<NoContentResponse>($"DoUndo()");
        }

        /// <summary>
        ///     Redo
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> Redo()
        {
            return await Send<NoContentResponse>($"DoRedo()");
        }

        /// <summary>
        ///     Saves soundlist
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> SaveSoundlist()
        {
            return await Send<NoContentResponse>($"DoSaveSoundlist()");
        }

        /// <summary>
        ///     Gets Soundpad's volume
        /// </summary>
        /// <returns></returns>
        public async Task<NumberResponse> GetVolume()
        {
            return await Send<NumberResponse>($"GetVolume()");
        }

        /// <summary>
        ///     Checks if Soundpad is muted
        /// </summary>
        /// <returns></returns>
        public async Task<BooleanResponse> IsMuted()
        {
            return await Send<BooleanResponse>($"IsMuted()");
        }

        /// <summary>
        ///     Sets volume
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> SetVolume(int volume)
        {
            return await Send<NoContentResponse>($"SetVolume({volume})");
        }

        /// <summary>
        ///     Toggles mute
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> ToggleMute()
        {
            return await Send<NoContentResponse>($"DoToggleMute()");
        }

        /// <summary>
        ///     Checks if connections is alive
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> IsAlive()
        {
            return await Send<NoContentResponse>($"IsAlive()");
        }

        /// <summary>
        ///     Get Soundpad's remote control version
        /// </summary>
        /// <returns></returns>
        public async Task<TextResponse> GetVersion()
        {
            return await Send<TextResponse>("GetRemoteControlVersion()");
        }

        /// <summary>
        ///     Plays selected sound
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> PlaySelectedSound()
        {
            return await Send<NoContentResponse>("DoPlaySelectedSound()");
        }

        /// <summary>
        ///     Plays the current sound again
        ///     https://www.youtube.com/watch?v=JiSS5xn_0fc
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> PlayCurrentSoundAgain()
        {
            return await Send<NoContentResponse>("DoPlayCurrentSoundAgain()");
        }

        /// <summary>
        ///     Plays the previously played sound
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> PlayPreviouslyPlayedSound()
        {
            return await Send<NoContentResponse>("DoPlayPreviouslyPlayedSound()");
        }

        /// <summary>
        ///     Adds a Category
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentIndex">-1 is bottom</param>
        /// <returns></returns>
        public async Task<NoContentResponse> AddCategory(string name, int parentIndex = -1)
        {
            return await Send<NoContentResponse>($"DoAddCategory(\"{name}\", {parentIndex})");
        }

        /// <summary>
        ///     Adds a sound to soudlist at given index in category
        /// </summary>
        /// <param name="path">Absolute file path</param>
        /// <param name="index"></param>
        /// <param name="categoryIndex"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> AddSound(string path, int index, int categoryIndex)
        {
            return await Send<NoContentResponse>($"DoAddSound(\"{path}\", {categoryIndex}, {index})");
        }

        /// <summary>
        ///     Starts recording of the speakers
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> StartRecordingSpeakers()
        {
            return await Send<NoContentResponse>("DoStartRecordingSpeakers()");
        }

        /// <summary>
        ///     Starts recording of the microphone
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> StartRecordingMicrophone()
        {
            return await Send<NoContentResponse>("DoStartRecordingMicrophone()");
        }

        /// <summary>
        ///     Selects category by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> SelectCategory(int index)
        {
            return await Send<NoContentResponse>($"DoSelectCategory({index})");
        }

        /// <summary>
        ///     Selects previous category
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> SelectPreviousCategory()
        {
            return await Send<NoContentResponse>("DoSelectPreviousCategory()");
        }

        /// <summary>
        ///     Selects next category
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> SelectNextCategory()
        {
            return await Send<NoContentResponse>("DoSelectNextCategory()");
        }

        /// <summary>
        ///     Removes category by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<NoContentResponse> RemoveCategory(int index)
        {
            return await Send<NoContentResponse>($"DoRemoveCategory({index})");
        }

        /// <summary>
        ///     Return categories
        /// </summary>
        /// <param name="withSounds"></param>
        /// <param name="withIcons"></param>
        /// <returns></returns>
        public async Task<CategoryListResponse> GetCategories(bool withSounds = false, bool withIcons = false)
        {
            return await Send<CategoryListResponse>($"GetCategories({withSounds}, {withIcons})");
        }

        /// <summary>
        ///     Return a category ba index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="withSounds"></param>
        /// <param name="withIcons"></param>
        /// <returns></returns>
        public async Task<CategoryResponse> GetCategory(int index, bool withSounds = false, bool withIcons = false)
        {
            return await Send<CategoryResponse>($"GetCategory({index}, {withSounds}, {withIcons})");
        }

        /// <summary>
        ///     Loads a .spl Soundlist into Soundpad
        /// </summary>
        /// <returns></returns>
        public async Task<NoContentResponse> LoadSoundlist(string path)
        {
            var loadSoundlist = new LoadSoundlist();
            return await loadSoundlist.Perform(path);
        }
    }
}