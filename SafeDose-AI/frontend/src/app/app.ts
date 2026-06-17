import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/services/auth';
import { Toast } from './shared/components/toast/toast';
import { ProgressBar } from './shared/components/progress-bar/progress-bar';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Toast,ProgressBar],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('frontend');

  protected readonly auth = inject(Auth);
}
