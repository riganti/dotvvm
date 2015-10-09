using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Projection
{
    internal class DothtmlProjectionTextViewModel : ITextViewModel
    {
        private ITextDataModel dataModel;
        private IProjectionBuffer projectionBuffer;

        public DothtmlProjectionTextViewModel(ITextDataModel dataModel, IProjectionBuffer projectionBuffer = null)
        {
            this.dataModel = dataModel;
            this.projectionBuffer = projectionBuffer;
        }

        public PropertyCollection Properties => projectionBuffer?.Properties ?? dataModel.DataBuffer.Properties;

        public ITextDataModel DataModel => dataModel;

        public ITextBuffer DataBuffer => dataModel.DataBuffer;

        public ITextBuffer EditBuffer => projectionBuffer ?? dataModel.DataBuffer;

        public ITextBuffer VisualBuffer => projectionBuffer ?? dataModel.DocumentBuffer;

        public void Dispose()
        {
        }

        public bool IsPointInVisualBuffer(SnapshotPoint editBufferPoint, PositionAffinity affinity)
        {
            return true;
        }

        public SnapshotPoint GetNearestPointInVisualBuffer(SnapshotPoint editBufferPoint)
        {
            return editBufferPoint;
        }

        public SnapshotPoint GetNearestPointInVisualSnapshot(SnapshotPoint editBufferPoint, ITextSnapshot targetVisualSnapshot, PointTrackingMode trackingMode)
        {
            return editBufferPoint.TranslateTo(targetVisualSnapshot, trackingMode);
        }
    }
}