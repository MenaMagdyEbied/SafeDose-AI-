import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Heart, LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-footer',
  imports: [LucideAngularModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  heartIcon = Heart;

  constructor(public router: Router) {}
}
