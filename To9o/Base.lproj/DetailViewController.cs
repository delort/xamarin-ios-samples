// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using UIKit;
using CoreSpotlight;

namespace StoryboardTables
{
	public partial class DetailViewController : UIViewController
	{
		Task current {get;set;}
		ContactHelper contacts;
		public CollectionController Delegate {get;set;}

		public DetailViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			SaveButton.TouchUpInside += (sender, e) => {
				current.Name = NameText.Text;
				current.Notes = NotesText.Text;
				current.Done = DoneSwitch.On;
				current.For = ForText.Text;

				Delegate.SaveTask(current);
				NavigationController.PopViewController(true);
			};
			CancelButton.TouchUpInside += (sender, e) => {
				if (Delegate != null)
					Delegate.DeleteTask(current);
				else 
					Console.WriteLine("Delegate not set - HACK");
				NavigationController.PopViewController(true);
			};
			contacts = new ContactHelper (current);
			UITapGestureRecognizer forTextTap = new UITapGestureRecognizer(() => {
				PresentViewController(contacts.GetPicker(),true, null);
			});
			ForText.AddGestureRecognizer(forTextTap);
			ForText.UserInteractionEnabled = true;

			NameText.TextAlignment = UITextAlignment.Natural;
			NotesText.TextAlignment = UITextAlignment.Natural;


			UserActivity = UserActivityHelper.CreateNSUserActivity (current?? new Task());
		}
		public override void ViewWillDisappear (bool animated)
		{
			UserActivity?.ResignCurrent ();

			base.ViewWillDisappear (animated);
		}
		// when displaying, set-up the properties
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NameText.Text = current.Name;
			NotesText.Text = current.Notes;
			DoneSwitch.On = current.Done;
			ForText.Text = current.For;

			// button is cancel or delete
			if (current.Id > 0) {
				CancelButton.SetTitle (NSBundle.MainBundle.LocalizedString ("Delete", "")
					, UIControlState.Normal);
				CancelButton.SetTitleColor (UIColor.Red, UIControlState.Normal);
			} else {
				CancelButton.SetTitle(NSBundle.MainBundle.LocalizedString ("Cancel", "")
					,UIControlState.Normal);
				CancelButton.SetTitleColor (UIColor.DarkTextColor, UIControlState.Normal);
			}
		}


		// this will be called before the view is displayed 
		public void SetTodo (Task todo) {
			current = todo;
		}

		#region Search & Handoff
		// this gets called periodically after activity.BecomeCurrent() is called
		// //http://www.raywenderlich.com/84174/ios-8-handoff-tutorial
		public override void UpdateUserActivityState (NSUserActivity activity)
		{
			Console.WriteLine ("UpdateUserActivityState for " + activity.Title);
			// update activity 
			if (current.IsIndexable()) {
				activity.AddUserInfoEntries (current.IdToDictionary());
				activity.AddUserInfoEntries (current.NameToDictionary());
			}
			base.UpdateUserActivityState (activity);
		}
		public override void RestoreUserActivityState (NSUserActivity activity)
		{
			base.RestoreUserActivityState (activity);
			Console.Write ("RestoreUserActivityState ");
			if ((activity.ActivityType == ActivityTypes.Detail) 
				|| (activity.ActivityType == ActivityTypes.Add))
			{
				Console.WriteLine ("NSUserActivity " + activity.ActivityType);
				if (activity.UserInfo == null || activity.UserInfo.Count == 0) {
					// new todo 
					current = new Task();
				} else {
					// load existing todo
					var id = activity.UserInfo.ObjectForKey (ActivityKeys.Id).ToString ();
					current = AppDelegate.Current.TaskMgr.GetTask (Convert.ToInt32 (id));
				}
			} 
			if (activity.ActivityType == CSSearchableItem.ActionType) {
				Console.WriteLine ("CSSearchableItem.ActionType");
				var uid = activity.UserInfo [CoreSpotlight.CSSearchableItem.ActivityIdentifier];

				current = AppDelegate.Current.TaskMgr.GetTask (Convert.ToInt32 (uid.Description));

				Console.WriteLine ("eeeeeeee RestoreUserActivityState " + uid);
			}

			// CoreSpotlight index can get out-of-date, show 'empty' task if the requested id is invalid
			if (current == null) {
				current = new Task { Name = "(not found)" };
			}
		}
		#endregion

		/// <summary>
		/// HACK: just a bit of fun
		/// </summary>
		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null)
			{
				var force = touch.Force;
				var maxForce = touch.MaximumPossibleForce;
				var alpha = force / maxForce;
				alpha = (nfloat)0.5 + (alpha / 2);
				View.BackgroundColor = UIColor.FromHSBA (1, 1, 1, alpha);
//				View.BackgroundColor = UIColor.Red.ColorWithAlpha (alpha);
			}
		}
	}
}
