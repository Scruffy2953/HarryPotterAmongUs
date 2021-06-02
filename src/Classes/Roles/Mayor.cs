using System;
using UnityEngine;
using System.Collections.Generic;
using HarryPotter.Classes.Helpers.UI;
using HarryPotter.Classes.UI;

namespace HarryPotter.Classes.Roles
{
	public class Mayor : Role
	{
		
		public Mayor(ModdedPlayerClass owner)
		{
			RoleName = "Mayor";
			RoleColor = Palette.Orange;
			RoleColor2 = Palette.Orange;
			IntroString = "We are going to test this.";
			Owner = owner;
		
			if (!owner._Object.AmOwner)
				return;
		}
		
		public override void Update()
		{
			if (!Owner._Object.AmOwner)
				return;
			
			if (!MudManager.Instance)
				return;
			
			DrawButtons();
		}
		
	}
}