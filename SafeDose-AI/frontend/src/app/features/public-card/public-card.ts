import { Component, DestroyRef, inject, OnInit } from '@angular/core';
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

  loading = false;
  error = '';

  private _cardData: CardData = {
    id: '',
    name: '',
    age: 0,
    medications: [],
    allergies: [],
    doctorName: '',
    qrUrl: '',
  };
  cardLoaded = false;

  loadCardData(token: string): void {
    this.loading = true;
    this.error = '';

    from(this.medicalCardService.getPublicCard(token))
      .pipe(
        catchError(() => {
          this.error = 'تعذر تحميل البطاقة الطبية.';
          return EMPTY;
        }),
        finalize(() => {
          this.loading = false;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((cardData) => {
        this._cardData = cardData;
        this.cardLoaded = true;
      });
  }
  get cardData(): CardData {
    return this._cardData;
  }

  ngOnInit() {
    const token = this.route.snapshot.params['token'];
    void this.loadCardData(token);
  }
}
