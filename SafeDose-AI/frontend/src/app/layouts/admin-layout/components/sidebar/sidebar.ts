import { Component } from '@angular/core';
import { Heart, LayoutDashboard, Menu, X } from 'lucide-angular';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Settings } from 'lucide-angular';
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
  mobileMenu = false;
}
