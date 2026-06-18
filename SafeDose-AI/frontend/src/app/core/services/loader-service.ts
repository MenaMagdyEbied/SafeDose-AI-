import { inject, Injectable, NgZone, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LoaderService {
  loading = signal(false);
  private activeRequests = 0;
  private readonly zone = inject(NgZone);

  show(): void {
    this.activeRequests++;
    this.update();
  }

  hide(): void {
    this.activeRequests = Math.max(0, this.activeRequests - 1);
    this.update();
  }

  private update(): void {
    const shouldShow = this.activeRequests > 0;
    if (this.loading() === shouldShow) return; 

    queueMicrotask(() => {
      this.zone.run(() => {
        this.loading.set(shouldShow);
      });
    });
  }
}
