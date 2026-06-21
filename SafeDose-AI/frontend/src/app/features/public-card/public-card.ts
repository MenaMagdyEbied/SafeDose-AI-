import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { EMPTY, from } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { MedicalCardService } from '../../core/services/medical-card';
import { CardData } from '../../core/models/card-data';
import { Card } from '../../shared/components/card/card';

@Component({
  selector: 'app-public-card',
  imports: [LucideAngularModule, FormsModule, Card],
  templateUrl: './public-card.html',
  styleUrl: './public-card.css',
})
export class PublicCard implements OnInit {
  protected readonly router = inject(Router);
  protected readonly route = inject(ActivatedRoute);
  private readonly medicalCardService = inject(MedicalCardService);
  private readonly destroyRef = inject(DestroyRef);

  loading = signal(false);
  error = signal('');
  cardData = signal<CardData>({
    id: '',
    name: '',
    age: 0,
    medications: [],
    allergies: [],
    doctorName: '',
    qrUrl: '',
  });
  cardLoaded = signal(false);

  loadCardData(token: string): void {
    this.loading.set(true);
    this.error.set('');
    this.cardLoaded.set(false);

    from(this.medicalCardService.getPublicCard(token))
      .pipe(
        catchError(() => {
          this.error.set('تعذر تحميل البطاقة الطبية.');
          return EMPTY;
        }),
        finalize(() => {
          this.loading.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((cardData) => {
        this.cardData.set(cardData);
        this.cardLoaded.set(true);
      });
  }

  ngOnInit() {
    const token = this.route.snapshot.params['token'];
    void this.loadCardData(token);
  }
}
