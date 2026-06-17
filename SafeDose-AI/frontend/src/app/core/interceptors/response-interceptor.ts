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
      // event هنا هو الـ body مباشرة (success, message, data) لأن مفيش observe: 'response'
      if (event?.message && event?.success && !isSilent) {
        toast.show('success', event.message);
      }
    }),
    catchError((err) => {
      const status = err.status;

      // شكل الإيرور برجاء التأكد من الباك إند هل بيرجع نفس شكل الـ success response
      // ({ success: false, message, data }) في حالة الأخطاء كمان ولا شكل مختلف
      const message: string =
        err?.error?.message ||
        err?.error?.errors?.[Object.keys(err?.error?.errors ?? {})[0]]?.[0] ||
        'حدث خطأ غير متوقع، حاول مرة أخرى.';

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
