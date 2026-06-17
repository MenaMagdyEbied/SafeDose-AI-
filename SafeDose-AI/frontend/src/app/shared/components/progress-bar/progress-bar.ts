import { Component, inject } from '@angular/core';
import { LoaderService } from '../../../core/services/loader-service';

@Component({
  selector: 'app-progress-bar',
  imports: [],
  templateUrl: './progress-bar.html',
  styleUrl: './progress-bar.css',
})
export class ProgressBar {
  loader = inject(LoaderService);
}
