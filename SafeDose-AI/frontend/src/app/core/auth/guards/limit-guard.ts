import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Subscription } from '../../services/subscription';

export const limitGuard: CanActivateFn = async (route, state) => {
  const sub = inject(Subscription);
  const router = inject(Router);

  const subscription = await sub.getSubscription();

  if (!subscription || subscription.tierCode === 'free') {
    return router.createUrlTree(['/pricing'], {
      queryParams: { reason: 'upgrade-required' },
    });
  }

  return true;
};
