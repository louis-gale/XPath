using System.Windows.Documents;

namespace Inverse.XPath
{
    internal static class TextPointerExtensions
    {
        internal static TextPointer GetOffsetTextPointer(this TextPointer start, int offset)
        {
            return start.GetInsertionPosition(LogicalDirection.Forward).GetPositionAtOffset(offset);
        }
    }
}