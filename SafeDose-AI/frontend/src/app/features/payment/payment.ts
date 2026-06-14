import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  CircleCheck,
  CreditCard,
  Lock,
  LucideAngularModule,
  Shield,
  Smartphone,
} from 'lucide-angular';

@Component({
  selector: 'app-payment',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment {
  creditCardIcon = CreditCard;
  smartphoneIcon = Smartphone;
  lockIcon = Lock;
  checkCircleIcon = CircleCheck;
  shieldIcon = Shield;

  method: 'card' | 'wallet' = 'card';
  showSuccess = false;

  card = { name: '', number: '', expiry: '', cvv: '' };
  wallet = { type: 'vodafone', phone: '' };

  wallets = [
    { id: 'vodafone', name: 'فودافون كاش', icon: '🔴' },
    { id: 'orange', name: 'اورنج كاش', icon: '🟠' },
    { id: 'etisalat', name: 'اتصالات كاش', icon: '🟢' },
  ];

  constructor(private router: Router) {}

  formatCardNumber(event: Event) {
    const input = event.target as HTMLInputElement;
    let val = input.value.replace(/\D/g, '').substring(0, 16);
    val = val.replace(/(.{4})/g, '$1 ').trim();
    this.card.number = val;
  }

  formatExpiry(event: Event) {
    const input = event.target as HTMLInputElement;
    let val = input.value.replace(/\D/g, '').substring(0, 4);
    if (val.length >= 2) val = val.substring(0, 2) + '/' + val.substring(2);
    this.card.expiry = val;
  }

  isValid(): boolean {
    if (this.method === 'card') {
      return (
        this.card.name.trim().length > 0 &&
        this.card.number.replace(/\s/g, '').length === 16 &&
        this.card.expiry.length === 5 &&
        this.card.cvv.length === 3
      );
    }
    return this.wallet.phone.length === 11 && this.wallet.phone.startsWith('01');
  }

  pay() {
    if (!this.isValid()) return;
    // TODO: POST /api/payment
    this.showSuccess = true;
  }

  goHome() {
    this.showSuccess = false;
    this.router.navigate(['/home']);
  }
}
