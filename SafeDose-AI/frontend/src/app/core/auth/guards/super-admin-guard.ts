import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../services/auth';

export const superAdminGuard: CanActivateFn = (route, state) => {
  const auth = inject(Auth);
  const router = inject(Router);
  if (auth.isLoggedIn && auth.isAdmin) return true;
  router.navigate([auth.isLoggedIn ? '/home' : '/login']);
  return false;
};
