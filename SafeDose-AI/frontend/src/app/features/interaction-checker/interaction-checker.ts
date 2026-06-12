import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  Activity,
  AlertTriangle,
  BookOpen,
  ChevronRight,
  LucideAngularModule,
  Pill,
  QrCode,

  Search,
  Sparkles,
  TriangleAlert,
  X
} from 'lucide-angular';
import { Interaction } from '../../core/services/interaction';

@Component({
  selector: 'app-interaction-checker',
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './interaction-checker.html',
  styleUrl: './interaction-checker.css',
})
export class InteractionChecker {
  private readonly router = inject(Router);
  private readonly interaction = inject(Interaction);
  searchWord = '';
  resultsOpen = false;
  loading = false;
  selectedMeds: string[] = [];

  pillIcon = Pill;
  searchIcon = Search;
  xIcon = X;
  bookOpenIcon = BookOpen;
  qrCodeIcon = QrCode;
  sparklesIcon = Sparkles;
  activityIcon = Activity;
  alertTriangleIcon = TriangleAlert;
  chevronRightIcon = ChevronRight;

  get filteredDrugs(): string[] {
    return this.interaction.searchDrugs(this.searchWord);
  }

  onSearchChange(val: string): void {
    this.searchWord = val;
    this.resultsOpen = val.length > 0 || true;
  }

  addMed(med: string): void {
    const clean = med.split(' (')[0].trim();
    if (this.selectedMeds.includes(clean) || this.selectedMeds.length >= 6) return;
    this.selectedMeds = [...this.selectedMeds, clean];
    this.searchWord = '';
    this.resultsOpen = false;
  }

  removeMed(index: number): void {
    this.selectedMeds = this.selectedMeds.filter((_, i) => i !== index);
  }

  loadFromProfile(): void {
    this.selectedMeds = ['ميتفورمين', 'وارفارين'];
  }

  scanBarcode(): void {
    this.selectedMeds = ['Aspirin', 'Warfarin'];
  }

  voiceInput(): void {
    this.selectedMeds = ['أسبرين', 'وارفارين'];
  }

  async runCheck(): Promise<void> {
    this.loading = true;
    const result = await this.interaction.checkInteractions(this.selectedMeds);
    sessionStorage.setItem('lastCheckedFlowOutput', JSON.stringify(result));
    this.loading = false;
    this.router.navigate(['/interaction-results']);
  }
}
