﻿namespace UIWidgets
{
	using System;
	using UnityEngine.Events;

	/// <summary>
	/// Draggable event.
	/// </summary>
	[Serializable]
	public class DraggableSnapEvent : UnityEvent<Draggable, SnapGridBase.Result>
	{
	}
}