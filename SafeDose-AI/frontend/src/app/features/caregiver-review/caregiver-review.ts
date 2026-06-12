import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  Camera,
  CircleCheck,
  LucideAngularModule,
  RotateCcw,
  TriangleAlert,
  Upload,
} from 'lucide-angular';

@Component({
  selector: 'app-caregiver-review',
  imports: [LucideAngularModule, RouterLink],
  templateUrl: './caregiver-review.html',
  styleUrl: './caregiver-review.css',
})
export class CaregiverReview {
  private readonly cdr = inject(ChangeDetectorRef);
  scanned = false;

  cameraIcon = Camera;
  uploadIcon = Upload;
  rotateCcwIcon = RotateCcw;
  alertTriangleIcon = TriangleAlert;
  checkCircleIcon = CircleCheck;
  videoStream: MediaStream | null = null;
  showCamera = false;

  async openCamera(): Promise<void> {
    try {
      this.showCamera = true;
      this.videoStream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' },
      });

      setTimeout(() => {
        const video = document.getElementById('cameraFeed') as HTMLVideoElement;
        if (video) video.srcObject = this.videoStream;
      }, 100);
    } catch (err) {
      this.showCamera = false;
    }
  }

  capturePhoto(): void {
    const video = document.getElementById('cameraFeed') as HTMLVideoElement;
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext('2d')?.drawImage(video, 0, 0);

    canvas.toBlob((blob) => {
      if (blob) {
        const file = new File([blob], 'prescription.jpg', { type: 'image/jpeg' });
        this.handleFile(file);
      }
    });

    this.closeCamera();
  }

  closeCamera(): void {
    this.videoStream?.getTracks().forEach((t) => t.stop());
    this.videoStream = null;
    this.showCamera = false;
  }

  openFilePicker(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = (e: any) => {
      const file = e.target.files[0];
      if (file) this.handleFile(file);
    };
    input.click();
  }
  handleFile(file: File): void {
    if (file) this.scanned = true;
    this.cdr.detectChanges();
  }
}
