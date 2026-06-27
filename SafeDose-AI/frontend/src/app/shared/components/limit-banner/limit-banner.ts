import { Component, inject, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Crown, Lock, LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-limit-banner',
  imports: [LucideAngularModule],
  templateUrl: './limit-banner.html',
  styleUrl: './limit-banner.css',
})
export class LimitBanner {
  private readonly router = inject(Router);

  @Input() title = 'هذه الميزة للمشتركين فقط';
  @Input() description = 'قم بالترقية للوصول لهذه الميزة';
  @Input() isPremium = false;

  crownIcon = Crown;
  lockIcon = Lock;

  upgrade() {
    this.router.navigate(['/pricing']);
  }
}
