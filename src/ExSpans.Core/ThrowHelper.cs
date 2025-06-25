using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Zyl.ExSpans.Impl;

namespace Zyl.ExSpans {

#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif // NET6_0_OR_GREATER
    internal static class ThrowHelper {

        private static ArgumentException GetArgumentException(ExceptionResource resource) {
            return new ArgumentException(GetResourceString(resource));
        }

        private static InvalidOperationException GetInvalidOperationException(ExceptionResource resource) {
            return new InvalidOperationException(GetResourceString(resource));
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException(ExceptionResource resource) {
            throw GetArgumentException(resource);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException_BadComparer(object? comparer) {
            throw new ArgumentException(SR.Format(SR.Arg_BogusIComparer, comparer));
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException_DestinationTooShort() {
            throw new ArgumentException(SR.Argument_DestinationTooShort, "destination");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException_OverlapAlignmentMismatch() {
            throw new ArgumentException(SR.Argument_OverlapAlignmentMismatch);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException() {
            throw new ArgumentOutOfRangeException();
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException(string paramName) {
            throw new ArgumentOutOfRangeException(paramName);
        }

        [DoesNotReturn]
        internal static void ThrowArrayTypeMismatchException() {
            throw new ArrayTypeMismatchException();
        }

        [DoesNotReturn]
        internal static void ThrowIndexOutOfRangeException() {
            throw new IndexOutOfRangeException();
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException() {
            throw new InvalidOperationException();
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException(ExceptionResource resource) {
            throw GetInvalidOperationException(resource);
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException(ExceptionResource resource, Exception e) {
            throw new InvalidOperationException(GetResourceString(resource), e);
        }

        [DoesNotReturn]
        internal static void ThrowInvalidTypeWithPointersNotSupported(Type targetType) {
            throw new ArgumentException(SR.Format(SR.Argument_InvalidTypeWithPointersNotSupported, targetType));
        }

        private static string GetResourceString(ExceptionResource resource) {
            switch (resource) {
#if TODO
                case ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual:
                    return SR.ArgumentOutOfRange_IndexMustBeLessOrEqual;
                case ExceptionResource.ArgumentOutOfRange_IndexMustBeLess:
                    return SR.ArgumentOutOfRange_IndexMustBeLess;
                case ExceptionResource.ArgumentOutOfRange_IndexCount:
                    return SR.ArgumentOutOfRange_IndexCount;
                case ExceptionResource.ArgumentOutOfRange_IndexCountBuffer:
                    return SR.ArgumentOutOfRange_IndexCountBuffer;
                case ExceptionResource.ArgumentOutOfRange_Count:
                    return SR.ArgumentOutOfRange_Count;
                case ExceptionResource.ArgumentOutOfRange_Year:
                    return SR.ArgumentOutOfRange_Year;
                case ExceptionResource.Arg_ArrayPlusOffTooSmall:
                    return SR.Arg_ArrayPlusOffTooSmall;
                case ExceptionResource.Arg_ByteArrayTooSmallForValue:
                    return SR.Arg_ByteArrayTooSmallForValue;
                case ExceptionResource.NotSupported_ReadOnlyCollection:
                    return SR.NotSupported_ReadOnlyCollection;
                case ExceptionResource.Arg_RankMultiDimNotSupported:
                    return SR.Arg_RankMultiDimNotSupported;
                case ExceptionResource.Arg_NonZeroLowerBound:
                    return SR.Arg_NonZeroLowerBound;
                case ExceptionResource.ArgumentOutOfRange_GetCharCountOverflow:
                    return SR.ArgumentOutOfRange_GetCharCountOverflow;
                case ExceptionResource.ArgumentOutOfRange_ListInsert:
                    return SR.ArgumentOutOfRange_ListInsert;
                case ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
                    return SR.ArgumentOutOfRange_NeedNonNegNum;
                case ExceptionResource.ArgumentOutOfRange_SmallCapacity:
                    return SR.ArgumentOutOfRange_SmallCapacity;
                case ExceptionResource.Argument_InvalidOffLen:
                    return SR.Argument_InvalidOffLen;
                case ExceptionResource.Argument_CannotExtractScalar:
                    return SR.Argument_CannotExtractScalar;
                case ExceptionResource.ArgumentOutOfRange_BiggerThanCollection:
                    return SR.ArgumentOutOfRange_BiggerThanCollection;
                case ExceptionResource.Serialization_MissingKeys:
                    return SR.Serialization_MissingKeys;
                case ExceptionResource.Serialization_NullKey:
                    return SR.Serialization_NullKey;
                case ExceptionResource.NotSupported_KeyCollectionSet:
                    return SR.NotSupported_KeyCollectionSet;
                case ExceptionResource.NotSupported_ValueCollectionSet:
                    return SR.NotSupported_ValueCollectionSet;
                case ExceptionResource.InvalidOperation_NullArray:
                    return SR.InvalidOperation_NullArray;
                case ExceptionResource.TaskT_TransitionToFinal_AlreadyCompleted:
                    return SR.TaskT_TransitionToFinal_AlreadyCompleted;
                case ExceptionResource.TaskCompletionSourceT_TrySetException_NullException:
                    return SR.TaskCompletionSourceT_TrySetException_NullException;
                case ExceptionResource.TaskCompletionSourceT_TrySetException_NoExceptions:
                    return SR.TaskCompletionSourceT_TrySetException_NoExceptions;
                case ExceptionResource.NotSupported_StringComparison:
                    return SR.NotSupported_StringComparison;
                case ExceptionResource.ConcurrentCollection_SyncRoot_NotSupported:
                    return SR.ConcurrentCollection_SyncRoot_NotSupported;
                case ExceptionResource.Task_MultiTaskContinuation_NullTask:
                    return SR.Task_MultiTaskContinuation_NullTask;
                case ExceptionResource.InvalidOperation_WrongAsyncResultOrEndCalledMultiple:
                    return SR.InvalidOperation_WrongAsyncResultOrEndCalledMultiple;
                case ExceptionResource.Task_MultiTaskContinuation_EmptyTaskList:
                    return SR.Task_MultiTaskContinuation_EmptyTaskList;
                case ExceptionResource.Task_Start_TaskCompleted:
                    return SR.Task_Start_TaskCompleted;
                case ExceptionResource.Task_Start_Promise:
                    return SR.Task_Start_Promise;
                case ExceptionResource.Task_Start_ContinuationTask:
                    return SR.Task_Start_ContinuationTask;
                case ExceptionResource.Task_Start_AlreadyStarted:
                    return SR.Task_Start_AlreadyStarted;
                case ExceptionResource.Task_RunSynchronously_Continuation:
                    return SR.Task_RunSynchronously_Continuation;
                case ExceptionResource.Task_RunSynchronously_Promise:
                    return SR.Task_RunSynchronously_Promise;
                case ExceptionResource.Task_RunSynchronously_TaskCompleted:
                    return SR.Task_RunSynchronously_TaskCompleted;
                case ExceptionResource.Task_RunSynchronously_AlreadyStarted:
                    return SR.Task_RunSynchronously_AlreadyStarted;
                case ExceptionResource.AsyncMethodBuilder_InstanceNotInitialized:
                    return SR.AsyncMethodBuilder_InstanceNotInitialized;
                case ExceptionResource.Task_ContinueWith_ESandLR:
                    return SR.Task_ContinueWith_ESandLR;
                case ExceptionResource.Task_ContinueWith_NotOnAnything:
                    return SR.Task_ContinueWith_NotOnAnything;
                case ExceptionResource.Task_InvalidTimerTimeSpan:
                    return SR.Task_InvalidTimerTimeSpan;
                case ExceptionResource.Task_Delay_InvalidMillisecondsDelay:
                    return SR.Task_Delay_InvalidMillisecondsDelay;
                case ExceptionResource.Task_Dispose_NotCompleted:
                    return SR.Task_Dispose_NotCompleted;
                case ExceptionResource.Task_ThrowIfDisposed:
                    return SR.Task_ThrowIfDisposed;
                case ExceptionResource.Task_WaitMulti_NullTask:
                    return SR.Task_WaitMulti_NullTask;
                case ExceptionResource.ArgumentException_OtherNotArrayOfCorrectLength:
                    return SR.ArgumentException_OtherNotArrayOfCorrectLength;
                case ExceptionResource.ArgumentNull_Array:
                    return SR.ArgumentNull_Array;
                case ExceptionResource.ArgumentNull_SafeHandle:
                    return SR.ArgumentNull_SafeHandle;
                case ExceptionResource.ArgumentOutOfRange_EndIndexStartIndex:
                    return SR.ArgumentOutOfRange_EndIndexStartIndex;
                case ExceptionResource.ArgumentOutOfRange_Enum:
                    return SR.ArgumentOutOfRange_Enum;
                case ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported:
                    return SR.ArgumentOutOfRange_HugeArrayNotSupported;
                case ExceptionResource.Argument_AddingDuplicate:
                    return SR.Argument_AddingDuplicate;
                case ExceptionResource.Argument_InvalidArgumentForComparison:
                    return SR.Argument_InvalidArgumentForComparison;
                case ExceptionResource.Arg_LowerBoundsMustMatch:
                    return SR.Arg_LowerBoundsMustMatch;
                case ExceptionResource.Arg_MustBeType:
                    return SR.Arg_MustBeType;
                case ExceptionResource.Arg_Need1DArray:
                    return SR.Arg_Need1DArray;
                case ExceptionResource.Arg_Need2DArray:
                    return SR.Arg_Need2DArray;
                case ExceptionResource.Arg_Need3DArray:
                    return SR.Arg_Need3DArray;
                case ExceptionResource.Arg_NeedAtLeast1Rank:
                    return SR.Arg_NeedAtLeast1Rank;
                case ExceptionResource.Arg_RankIndices:
                    return SR.Arg_RankIndices;
                case ExceptionResource.Arg_RanksAndBounds:
                    return SR.Arg_RanksAndBounds;
#endif // (TODO)
                case ExceptionResource.InvalidOperation_IComparerFailed:
                    return SR.InvalidOperation_IComparerFailed;
#if TODO
                case ExceptionResource.NotSupported_FixedSizeCollection:
                    return SR.NotSupported_FixedSizeCollection;
                case ExceptionResource.Rank_MultiDimNotSupported:
                    return SR.Rank_MultiDimNotSupported;
                case ExceptionResource.Arg_TypeNotSupported:
                    return SR.Arg_TypeNotSupported;
#endif // (TODO)
                case ExceptionResource.Argument_SpansMustHaveSameLength:
                    return SR.Argument_SpansMustHaveSameLength;
#if TODO
                case ExceptionResource.Argument_InvalidFlag:
                    return SR.Argument_InvalidFlag;
                case ExceptionResource.CancellationTokenSource_Disposed:
                    return SR.CancellationTokenSource_Disposed;
                case ExceptionResource.Argument_AlignmentMustBePow2:
                    return SR.Argument_AlignmentMustBePow2;
                case ExceptionResource.ArgumentOutOfRange_NotGreaterThanBufferLength:
                    return SR.ArgumentOutOfRange_NotGreaterThanBufferLength;
#endif // (TODO)
                case ExceptionResource.InvalidOperation_SpanOverlappedOperation:
                    return SR.InvalidOperation_SpanOverlappedOperation;
#if TODO
                case ExceptionResource.InvalidOperation_TimeProviderNullLocalTimeZone:
                    return SR.InvalidOperation_TimeProviderNullLocalTimeZone;
                case ExceptionResource.InvalidOperation_TimeProviderInvalidTimestampFrequency:
                    return SR.InvalidOperation_TimeProviderInvalidTimestampFrequency;
                case ExceptionResource.Format_UnexpectedClosingBrace:
                    return SR.Format_UnexpectedClosingBrace;
                case ExceptionResource.Format_UnclosedFormatItem:
                    return SR.Format_UnclosedFormatItem;
                case ExceptionResource.Format_ExpectedAsciiDigit:
                    return SR.Format_ExpectedAsciiDigit;
                case ExceptionResource.Argument_HasToBeArrayClass:
                    return SR.Argument_HasToBeArrayClass;
                case ExceptionResource.InvalidOperation_IncompatibleComparer:
                    return SR.InvalidOperation_IncompatibleComparer;
#endif // (TODO)
                default:
                    DebugHelper.Fail("The enum value is not defined, please check the ExceptionResource Enum.");
                    return "";
            }
        }
    }

    //
    // The convention for this enum is using the resource name as the enum name
    //
    internal enum ExceptionResource {
        ArgumentOutOfRange_IndexMustBeLessOrEqual,
        ArgumentOutOfRange_IndexMustBeLess,
        ArgumentOutOfRange_IndexCount,
        ArgumentOutOfRange_IndexCountBuffer,
        ArgumentOutOfRange_Count,
        ArgumentOutOfRange_Year,
        Arg_ArrayPlusOffTooSmall,
        Arg_ByteArrayTooSmallForValue,
        NotSupported_ReadOnlyCollection,
        Arg_RankMultiDimNotSupported,
        Arg_NonZeroLowerBound,
        ArgumentOutOfRange_GetCharCountOverflow,
        ArgumentOutOfRange_ListInsert,
        ArgumentOutOfRange_NeedNonNegNum,
        ArgumentOutOfRange_NotGreaterThanBufferLength,
        ArgumentOutOfRange_SmallCapacity,
        Argument_InvalidOffLen,
        Argument_CannotExtractScalar,
        ArgumentOutOfRange_BiggerThanCollection,
        Serialization_MissingKeys,
        Serialization_NullKey,
        NotSupported_KeyCollectionSet,
        NotSupported_ValueCollectionSet,
        InvalidOperation_NullArray,
        TaskT_TransitionToFinal_AlreadyCompleted,
        TaskCompletionSourceT_TrySetException_NullException,
        TaskCompletionSourceT_TrySetException_NoExceptions,
        NotSupported_StringComparison,
        ConcurrentCollection_SyncRoot_NotSupported,
        Task_MultiTaskContinuation_NullTask,
        InvalidOperation_WrongAsyncResultOrEndCalledMultiple,
        Task_MultiTaskContinuation_EmptyTaskList,
        Task_Start_TaskCompleted,
        Task_Start_Promise,
        Task_Start_ContinuationTask,
        Task_Start_AlreadyStarted,
        Task_RunSynchronously_Continuation,
        Task_RunSynchronously_Promise,
        Task_RunSynchronously_TaskCompleted,
        Task_RunSynchronously_AlreadyStarted,
        AsyncMethodBuilder_InstanceNotInitialized,
        Task_ContinueWith_ESandLR,
        Task_ContinueWith_NotOnAnything,
        Task_InvalidTimerTimeSpan,
        Task_Delay_InvalidMillisecondsDelay,
        Task_Dispose_NotCompleted,
        Task_ThrowIfDisposed,
        Task_WaitMulti_NullTask,
        ArgumentException_OtherNotArrayOfCorrectLength,
        ArgumentNull_Array,
        ArgumentNull_SafeHandle,
        ArgumentOutOfRange_EndIndexStartIndex,
        ArgumentOutOfRange_Enum,
        ArgumentOutOfRange_HugeArrayNotSupported,
        Argument_AddingDuplicate,
        Argument_InvalidArgumentForComparison,
        Arg_LowerBoundsMustMatch,
        Arg_MustBeType,
        Arg_Need1DArray,
        Arg_Need2DArray,
        Arg_Need3DArray,
        Arg_NeedAtLeast1Rank,
        Arg_RankIndices,
        Arg_RanksAndBounds,
        InvalidOperation_IComparerFailed,
        NotSupported_FixedSizeCollection,
        Rank_MultiDimNotSupported,
        Arg_TypeNotSupported,
        Argument_SpansMustHaveSameLength,
        Argument_InvalidFlag,
        CancellationTokenSource_Disposed,
        Argument_AlignmentMustBePow2,
        InvalidOperation_SpanOverlappedOperation,
        InvalidOperation_TimeProviderNullLocalTimeZone,
        InvalidOperation_TimeProviderInvalidTimestampFrequency,
        Format_UnexpectedClosingBrace,
        Format_UnclosedFormatItem,
        Format_ExpectedAsciiDigit,
        Argument_HasToBeArrayClass,
        InvalidOperation_IncompatibleComparer,
    }

}
