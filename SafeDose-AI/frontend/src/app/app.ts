import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/services/auth';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('frontend');

  protected readonly auth = inject(Auth);
}
