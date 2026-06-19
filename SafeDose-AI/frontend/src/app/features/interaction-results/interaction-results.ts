import { Component, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CircleCheck, Info, LucideAngularModule, Phone, Pill, TriangleAlert } from 'lucide-angular';
import { InteractionResult } from '../../core/models';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-interaction-results',
  imports: [LucideAngularModule, RouterLink, CommonModule],
  templateUrl: './interaction-results.html',
  styleUrl: './interaction-results.css',
})
export class InteractionResults {
  data = signal<InteractionResult | null>(null);

  alertTriangleIcon = TriangleAlert;
  phoneIcon = Phone;
  pillIcon = Pill;
  checkCircleIcon = CircleCheck;
  infoIcon = Info;

  constructor(public router: Router) {}

  ngOnInit(): void {
    const raw = sessionStorage.getItem('lastCheckedFlowOutput');
    if (raw) {
      try {
        this.data.set(JSON.parse(raw));
      } catch {
        this.data.set(null);
      }
    }
  }

  get levelIcon() {
    const current = this.data();
    if (!current) return this.infoIcon;
    if (current.level >= 3) return this.alertTriangleIcon;
    if (current.level === 0) return this.checkCircleIcon;
    return this.infoIcon;
  }

  checkAnother(): void {
    sessionStorage.removeItem('lastCheckedFlowOutput');
    this.router.navigate(['/interaction-checker']);
  }
}
