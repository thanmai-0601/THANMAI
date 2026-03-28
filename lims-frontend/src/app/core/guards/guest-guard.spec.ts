import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { guestGuard } from './guest-guard';
import { AuthService } from '../services/auth';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

describe('guestGuard', () => {
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(() => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn', 'getDashboardRoute']);
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        TestBed.configureTestingModule({
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy }
            ]
        });
    });

    it('should allow access when user is not logged in', () => {
        authServiceSpy.isLoggedIn.and.returnValue(false);
        const result = TestBed.runInInjectionContext(() =>
            guestGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
        );
        expect(result).toBeTrue();
    });

    it('should deny access and redirect when user is logged in', () => {
        authServiceSpy.isLoggedIn.and.returnValue(true);
        authServiceSpy.getDashboardRoute.and.returnValue('/app/dashboard/customer');
        
        const result = TestBed.runInInjectionContext(() =>
            guestGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
        );
        
        expect(result).toBeFalse();
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/app/dashboard/customer']);
    });
});
