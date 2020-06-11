using System;

namespace Sttz.Tweener.Core {

/// <summary>
/// Formatted string that evaluated lazily and doesn't use params.
/// </summary>
/// <remarks>
/// Formatted strings are nice for debug messages but both the
/// formatting and the params method allocate memory. This helper
/// struct allows to defer this allocation until it's necessary,
/// to avoid the allocation for e.g. debug messages that are
/// not logged due to the current log level.
/// </remarks>
public struct LazyFormatString
{
	public string format;
	public short args;
	public object arg1;
	public object arg2;
	public object arg3;
	public object arg4;
	public object arg5;

	public LazyFormatString(string format)
	{
		this.format = format;
		this.args = 0;
		this.arg1 = this.arg2 = this.arg3 = this.arg4 = this.arg5 = null;
	}

	public LazyFormatString(string format, object arg1)
	{
		this.format = format;
		this.args = 1;
		this.arg1 = arg1;
		this.arg2 = this.arg3 = this.arg4 = this.arg5 = null;
	}

	public LazyFormatString(string format, object arg1, object arg2)
	{
		this.format = format;
		this.args = 2;
		this.arg1 = arg1;
		this.arg2 = arg2;
		this.arg3 = this.arg4 = this.arg5 = null;
	}

	public LazyFormatString(string format, object arg1, object arg2, object arg3)
	{
		this.format = format;
		this.args = 3;
		this.arg1 = arg1;
		this.arg2 = arg2;
		this.arg3 = arg3;
		this.arg4 = this.arg5 = null;
	}

	public LazyFormatString(string format, object arg1, object arg2, object arg3, object arg4)
	{
		this.format = format;
		this.args = 4;
		this.arg1 = arg1;
		this.arg2 = arg2;
		this.arg3 = arg3;
		this.arg4 = arg4;
		this.arg5 = null;
	}

	public LazyFormatString(string format, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		this.format = format;
		this.args = 5;
		this.arg1 = arg1;
		this.arg2 = arg2;
		this.arg3 = arg3;
		this.arg4 = arg4;
		this.arg5 = arg5;
	}

	public override string ToString()
	{
		switch (args) {
			case 0:
				return format;
			case 1:
				return string.Format(format, arg1);
			case 2:
				return string.Format(format, arg1, arg2);
			case 3:
				return string.Format(format, arg1, arg2, arg3);
			case 4:
				return string.Format(format, arg1, arg2, arg3, arg4);
			case 5:
				return string.Format(format, arg1, arg2, arg3, arg4, arg5);
			default:
				throw new Exception($"FormatString: Invalid arguments count: {args}");
		}
	}

	public static implicit operator LazyFormatString(string format)
	{
		return new LazyFormatString(format);
	}
}

public static class FormatStringExtension
{
	public static LazyFormatString LazyFormat(this string format)
	{
		return new LazyFormatString(format);
	}

	public static LazyFormatString LazyFormat(this string format, object arg1)
	{
		return new LazyFormatString(format, arg1);
	}

	public static LazyFormatString LazyFormat(this string format, object arg1, object arg2)
	{
		return new LazyFormatString(format, arg1, arg2);
	}

	public static LazyFormatString LazyFormat(this string format, object arg1, object arg2, object arg3)
	{
		return new LazyFormatString(format, arg1, arg2, arg3);
	}

	public static LazyFormatString LazyFormat(this string format, object arg1, object arg2, object arg3, object arg4)
	{
		return new LazyFormatString(format, arg1, arg2, arg3, arg4);
	}

	public static LazyFormatString LazyFormat(this string format, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		return new LazyFormatString(format, arg1, arg2, arg3, arg4, arg5);
	}
}

}
