import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastService } from '../services/toast-service';
import { Auth } from '../auth/services/auth';
import { Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';
const SILENT_SUCCESS_URLS = ['/profile'];
export const responseInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const auth = inject(Auth);
  const router = inject(Router);

  const isSilent = req.method === 'GET' || SILENT_SUCCESS_URLS.some((url) => req.url.includes(url));

  return next(req).pipe(
    tap((event: any) => {
      if (event?.body?.message && !isSilent) {
        toast.show('success', event.body.message);
      }
      if (event?.body?.messageArabic && !isSilent) {
        toast.show('success', event.body.messageArabic);
      }
    }),
    catchError((err) => {
      const status = err.status;

      const message: string =
        err?.error?.message || err?.error?.messageArabic || 'حدث خطأ غير متوقع، حاول مرة أخرى.';

      if (status === 401) {
        auth.logout();
        router.navigate(['/login']);
        toast.show('error', 'انتهت جلستك، سجّل الدخول مرة أخرى.');
      } else if (status === 403) {
        toast.show('error', 'ليس لديك صلاحية للوصول لهذا المحتوى.');
      } else if (status === 0) {
        toast.show('error', 'تعذّر الاتصال بالخادم، تحقق من الإنترنت.');
      } else {
        toast.show('error', message);
      }

      return throwError(() => err);
    }),
  );
};
