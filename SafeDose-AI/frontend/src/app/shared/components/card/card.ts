import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  Heart,
  LucideAngularModule,
  Pill,
  Plus,
  Printer,
  QrCode,
  Shield,
  SquarePen,
  Trash2,
  X,
} from 'lucide-angular';
import { CardData } from '../../../core/models/card-data';

@Component({
  selector: 'app-card',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './card.html',
  styleUrl: './card.css',
})
export class Card {
  @Input() cardData!: CardData;

  heartIcon = Heart;
  pillIcon = Pill;
  qrCodeIcon = QrCode;
  printerIcon = Printer;
  shieldIcon = Shield;
  editIcon = SquarePen;
  trashIcon = Trash2;
  plusIcon = Plus;
  xIcon = X;
}
