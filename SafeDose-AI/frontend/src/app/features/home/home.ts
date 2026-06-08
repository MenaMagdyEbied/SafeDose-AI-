import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  Activity,
  AlertTriangle,
  Heart,
  LucideAngularModule,
  Pill,
  Shield,
  Sparkles,
} from 'lucide-angular';

@Component({
  selector: 'app-home',
  imports: [LucideAngularModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected readonly router = inject(Router);

  shieldIcon = Shield;
  pillIcon = Pill;
  sparklesIcon = Sparkles;
  activityIcon = Activity;
  alertIcon = AlertTriangle;
  heartIcon = Heart;
}
