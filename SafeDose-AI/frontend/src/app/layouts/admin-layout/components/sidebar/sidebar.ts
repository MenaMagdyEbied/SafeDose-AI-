import { Component } from '@angular/core';
import { Heart, LayoutDashboard, Menu, Settings, X, Users } from 'lucide-angular';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  settingsIcon = Settings;
  layoutDashboardIcon = LayoutDashboard;
  heartIcon = Heart;
  menuIcon = Menu;
  xIcon = X;
  usersIcon = Users;
  mobileMenu = false;
}
