import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { Auth } from '../../core/auth/services/auth';
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
  private readonly auth = inject(Auth);

  private _cardData: CardData = {
    id: '',
    name: '',
    age: 0,
    medications: [],
    allergies: [],
    doctorName: '',
    qrUrl: '',
  };

  get cardData(): CardData {
    return this._cardData;
  }

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.loadCardData(id);
  }

  loadCardData(id: string) {
    // TODO: استبدليه بـ API call حقيقي
    this._cardData = {
      id: id,
      name: 'دعاء أشرف',
      age: 30,
      medications: [
        { name: 'جلوكوفاج', dose: '500 ملجم', frequency: 'مرتان يومياً', startDate: '' },
        { name: 'كونكور', dose: '5 ملجم', frequency: 'مرة مساءً', startDate: '' },
      ],
      allergies: ['لا يوجد'],
      doctorName: 'د. مجدي يعقوب',
      qrUrl: `https://yourdomain.com/card/${id}`,
    };
  }
}
