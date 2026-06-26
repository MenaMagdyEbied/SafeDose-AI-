import { inject } from '@angular/core';
import { CanActivateFn, RouterEvent } from '@angular/router';
import { Auth } from '../services/auth';
import { Router } from '@angular/router';

export const adminGuard: CanActivateFn = (route, state) => {
  const auth = inject(Auth);
  const router = inject(Router);
  if (auth.isLoggedIn && auth.isAdmin) return true;
  return router.createUrlTree([auth.isLoggedIn ? '/home' : '/login']);
};
