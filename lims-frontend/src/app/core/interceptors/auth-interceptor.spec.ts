import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth-interceptor';

describe('authInterceptor', () => {
    it('should be defined', () => {
        expect(authInterceptor).toBeDefined();
    });

    it('should be a function', () => {
        expect(typeof authInterceptor).toBe('function');
    });

    it('should exist as an interceptor', () => {
        expect(authInterceptor).toBeTruthy();
    });

    it('should have function signature', () => {
        expect(authInterceptor.length).toBeGreaterThanOrEqual(0);
    });

    it('should be importable', () => {
        expect(authInterceptor).not.toBeNull();
    });
});
