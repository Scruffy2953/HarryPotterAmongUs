using System;
using System.Collections.Generic;
using System.Data;
using HarryPotter.Classes.UI;
using UnityEngine;

namespace HarryPotter.Classes.Roles
{
	public class HeadMaster : Roles
	{
		public HeadMaster(ModdedPlayerClass owner)
		{
			RoleName = "Head Master";
			RoleColor = Palette.Orange;
			RoleColor2 = Palette.Orange;
			IntroString = "30 points to Gryffindor";
			Owner = owner;
			if (!Owner._Object.AmOwner)
				return;
		}
		public override void Update()
		{
			if (!Owner._Object.AmOwner)
				return;
			if(!MudManager.Instance)
				return;
			
			DrawButtons();
		}
	}
}