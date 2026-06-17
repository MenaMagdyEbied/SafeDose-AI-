import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LoaderService {loading = signal(false);
  private activeRequests = 0;

  show(): void {
    this.activeRequests++;
    this.loading.set(true);
  }

  hide(): void {
    this.activeRequests = Math.max(0, this.activeRequests - 1);
    if (this.activeRequests === 0) {
      this.loading.set(false);
    }
  }
}
