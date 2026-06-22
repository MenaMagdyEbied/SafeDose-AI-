import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../services/auth';

export const guestGuard: CanActivateFn = (route, state) => {
  const auth = inject(Auth);
  const router = inject(Router);
  if (!auth.isLoggedIn) return true;

  // Already logged in — bounce to the right home instead of letting login/register show.
  return auth.isAdmin
    ? router.createUrlTree(['/admin'])
    : router.createUrlTree(['/home']);
};
