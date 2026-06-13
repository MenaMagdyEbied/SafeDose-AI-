import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Heart, LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-footer',
  imports: [LucideAngularModule, RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  heartIcon = Heart;

}
