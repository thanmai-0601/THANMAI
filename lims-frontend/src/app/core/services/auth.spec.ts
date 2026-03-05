import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth';
import { ApiService } from './api';

describe('AuthService', () => {
    let service: AuthService;

    beforeEach(() => {
        localStorage.clear();
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                AuthService,
                ApiService,
                { provide: Router, useValue: jasmine.createSpyObj('Router', ['navigate']) }
            ]
        });
        service = TestBed.inject(AuthService);
    });

    afterEach(() => {
        localStorage.clear();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should return null token when not logged in', () => {
        expect(service.getToken()).toBeNull();
    });

    it('should return false for isLoggedIn when no user stored', () => {
        expect(service.isLoggedIn()).toBeFalse();
    });

    it('should return null for getUserRole when not logged in', () => {
        expect(service.getUserRole()).toBeNull();
    });

    it('should return null for getUserName when not logged in', () => {
        expect(service.getUserName()).toBeNull();
    });

    it('should have login method', () => {
        expect(service.login).toBeDefined();
    });

    it('should have logout method', () => {
        expect(service.logout).toBeDefined();
    });
});
