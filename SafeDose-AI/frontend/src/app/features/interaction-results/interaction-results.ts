import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { LucideAngularModule, Phone, Pill, TriangleAlert } from 'lucide-angular';
import { InteractionResult } from '../../core/models';

@Component({
  selector: 'app-interaction-results',
  imports: [LucideAngularModule, RouterLink],
  templateUrl: './interaction-results.html',
  styleUrl: './interaction-results.css',
})
export class InteractionResults {
  data: InteractionResult | null = null;

  alertTriangleIcon = TriangleAlert;
  phoneIcon = Phone;
  pillIcon = Pill;

  constructor(public router: Router) {}

  ngOnInit(): void {
    const raw = sessionStorage.getItem('lastCheckedFlowOutput');
    if (raw) {
      try {
        this.data = JSON.parse(raw);
      } catch {}
    }
  }
}
