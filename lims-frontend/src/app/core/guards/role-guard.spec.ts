import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { roleGuard } from './role-guard';
import { AuthService } from '../services/auth';

describe('roleGuard', () => {
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(() => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['getUserRole', 'getDashboardRoute']);
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        TestBed.configureTestingModule({
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy }
            ]
        });
    });

    function createRoute(roles: string[]): ActivatedRouteSnapshot {
        return { data: { roles } } as unknown as ActivatedRouteSnapshot;
    }

    it('should allow access when user role matches', () => {
        authServiceSpy.getUserRole.and.returnValue('Admin');
        const route = createRoute(['Admin', 'Agent']);
        const result = TestBed.runInInjectionContext(() =>
            roleGuard(route, {} as RouterStateSnapshot)
        );
        expect(result).toBeTrue();
    });

    it('should deny access when user role does not match', () => {
        authServiceSpy.getUserRole.and.returnValue('Customer');
        authServiceSpy.getDashboardRoute.and.returnValue('/app/dashboard/customer');
        const route = createRoute(['Admin']);
        const result = TestBed.runInInjectionContext(() =>
            roleGuard(route, {} as RouterStateSnapshot)
        );
        expect(result).toBeFalse();
    });

    it('should redirect to dashboard when role mismatch', () => {
        authServiceSpy.getUserRole.and.returnValue('Customer');
        authServiceSpy.getDashboardRoute.and.returnValue('/app/dashboard/customer');
        const route = createRoute(['Admin']);
        TestBed.runInInjectionContext(() =>
            roleGuard(route, {} as RouterStateSnapshot)
        );
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/app/dashboard/customer']);
    });

    it('should deny access when role is null', () => {
        authServiceSpy.getUserRole.and.returnValue(null);
        authServiceSpy.getDashboardRoute.and.returnValue('/login');
        const route = createRoute(['Admin']);
        const result = TestBed.runInInjectionContext(() =>
            roleGuard(route, {} as RouterStateSnapshot)
        );
        expect(result).toBeFalse();
    });

    it('should allow Agent role for agent-only routes', () => {
        authServiceSpy.getUserRole.and.returnValue('Agent');
        const route = createRoute(['Agent']);
        const result = TestBed.runInInjectionContext(() =>
            roleGuard(route, {} as RouterStateSnapshot)
        );
        expect(result).toBeTrue();
    });
});
