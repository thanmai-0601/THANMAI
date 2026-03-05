import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const expectedRoles: string[] = route.data['roles'];
  const userRole = authService.getUserRole();

  if (userRole && expectedRoles.includes(userRole)) {
    return true;
  }

  // Redirect to their own dashboard
  router.navigate([authService.getDashboardRoute()]);
  return false;
};
