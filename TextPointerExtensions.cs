using System.Windows.Documents;

namespace Inverse.XPath
{
    internal static class TextPointerExtensions
    {
        internal static TextPointer GetOffsetTextPointer(this TextPointer start, int offset)
        {
            var ret = start;
            var i = 0;

            while (i < offset && ret != null)
            {
                if (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text || ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None)
                {
                    i++;
                }

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                {
                    return ret;
                }

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }
    }
}