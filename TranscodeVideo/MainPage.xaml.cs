using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Audio;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TranscodeVideo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
           
        }

        StorageFile source, destination;
        uint Source_height, Source_width,Source_frameRate, SampleRate, dataRate;
        VideoEncodingProperties property;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            destination = await ApplicationData.Current.LocalFolder.CreateFileAsync("myfile1",CreationCollisionOption.ReplaceExisting);
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".flv");
            openPicker.FileTypeFilter.Add(".3gp");
            openPicker.FileTypeFilter.Add(".avi");
            openPicker.FileTypeFilter.Add(".mkv");
            source = await openPicker.PickSingleFileAsync();

            //获取视频信息
            if (source != null)
            {
                var clip = await MediaClip.CreateFromFileAsync(source);
                property = clip.GetVideoEncodingProperties();
                string encodeInfo = property.Subtype;
                Source_height = property.Height;
                Source_width = property.Width;
                
                Source_frameRate = property.FrameRate.Numerator;
                dataRate = property.Bitrate;
            }
            //获取声频信息
            if (source != null)
            {
                AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
                CreateAudioGraphResult createResult = await AudioGraph.CreateAsync(settings);
                if (createResult.Status != AudioGraphCreationStatus.Success)
                {
                    return;
                }
                AudioGraph audioGraph = createResult.Graph;
                CreateAudioFileInputNodeResult result = await audioGraph.CreateFileInputNodeAsync(source);
                if (result.Status != AudioFileNodeCreationStatus.Success)
                {
                    return;
                }
                AudioFileInputNode fileInputNode = result.FileInputNode;
                AudioEncodingProperties property = fileInputNode.EncodingProperties;
                //Get encode property
                string subTitles = property.Subtype;
                SampleRate = property.SampleRate;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //MediaEncodingProfile profile = new MediaEncodingProfile();
            //profile.Video = property;

            //profile.Video.Subtype = MediaEncodingSubtypes.H264;
            //profile.Video.Height = Source_height;
            //profile.Video.Width = Source_width;
            //profile.Video.FrameRate.Numerator = Source_frameRate;
            //profile.Audio.Subtype = MediaEncodingSubtypes.Aac;
            //profile.Audio.SampleRate = SampleRate;


            MediaEncodingProfile profile = await MediaEncodingProfile.CreateFromFileAsync(source);
            profile.Video.Subtype = MediaEncodingSubtypes.H264;
            profile.Video.Height = Source_height;
            profile.Video.Width = Source_width;
            profile.Video.FrameRate.Numerator = Source_frameRate;
            profile.Audio.SampleRate = SampleRate;
            profile.Audio.Subtype = MediaEncodingSubtypes.Aac;
            MediaTranscoder transcoder = new MediaTranscoder();

            //MediaEncodingProfile profile = new MediaEncodingProfile();
            //profile.Video.Subtype = MediaEncodingSubtypes.H264;
            //profile.Video.Height = Source_height;
            //profile.Video.Width = Source_width;
            //profile.Video.FrameRate.Numerator = Source_frameRate;
            //profile.Video.Bitrate = property.Bitrate;
            //profile.Audio.SampleRate = SampleRate;
            //profile.Audio.Subtype = MediaEncodingSubtypes.Aac;
            //MediaTranscoder transcoder = new MediaTranscoder();
            PrepareTranscodeResult prepareOp = await
                transcoder.PrepareFileTranscodeAsync(source, destination, profile);
            if (prepareOp.CanTranscode)
            {
                var transcodeOp = prepareOp.TranscodeAsync();
                transcodeOp.Progress +=
                    new AsyncActionProgressHandler<double>(TranscodeProgress);
                transcodeOp.Completed +=
                    new AsyncActionWithProgressCompletedHandler<double>(TranscodeComplete);
            }
            else
            {
                switch (prepareOp.FailureReason)
                {
                    case TranscodeFailureReason.CodecNotFound:
                        System.Diagnostics.Debug.WriteLine("Codec not found.");
                        break;
                    case TranscodeFailureReason.InvalidProfile:
                        System.Diagnostics.Debug.WriteLine("Invalid profile.");
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unknown failure.");
                        break;
                }
            }
        }

        private void TranscodeComplete(IAsyncActionWithProgress<double> asyncInfo, AsyncStatus asyncStatus)
        {
            Debug.WriteLine("TranscodeComplete");
        }

        private void TranscodeProgress(IAsyncActionWithProgress<double> asyncInfo, double progressInfo)
        {
            Debug.WriteLine("TranscodeProgress");
        }
    }
}
