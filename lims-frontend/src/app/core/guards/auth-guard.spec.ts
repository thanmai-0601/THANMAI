import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth-guard';
import { AuthService } from '../services/auth';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

describe('authGuard', () => {
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(() => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn']);
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        TestBed.configureTestingModule({
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy }
            ]
        });
    });

    it('should allow access when user is logged in', () => {
        authServiceSpy.isLoggedIn.and.returnValue(true);
        const result = TestBed.runInInjectionContext(() =>
            authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
        );
        expect(result).toBeTrue();
    });

    it('should deny access when user is not logged in', () => {
        authServiceSpy.isLoggedIn.and.returnValue(false);
        const result = TestBed.runInInjectionContext(() =>
            authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
        );
        expect(result).toBeFalse();
    });

    it('should redirect to /login when not authenticated', () => {
        authServiceSpy.isLoggedIn.and.returnValue(false);
        TestBed.runInInjectionContext(() =>
            authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
        );
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should not redirect when authenticated', () => {
        authServiceSpy.isLoggedIn.and.returnValue(true);
        TestBed.runInInjectionContext(() =>
            authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
        );
        expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should call isLoggedIn on AuthService', () => {
        authServiceSpy.isLoggedIn.and.returnValue(true);
        TestBed.runInInjectionContext(() =>
            authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
        );
        expect(authServiceSpy.isLoggedIn).toHaveBeenCalled();
    });
});
