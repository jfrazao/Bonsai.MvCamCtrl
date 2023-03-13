using MvCamCtrl.NET;
using OpenCV.Net;

namespace Bonsai.MvCamCtrl
{
    public class MvCamCtrlDataFrame
    {
        public MvCamCtrlDataFrame(IplImage image, CFrameout chunkData)
        {
            Image = image;
            ChunkData = chunkData;
        }

        public IplImage Image { get; private set; }

        public CFrameout ChunkData { get; private set; }
    }
}
