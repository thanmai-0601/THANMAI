import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme';

describe('ThemeService', () => {
    let service: ThemeService;

    beforeEach(() => {
        localStorage.clear();
        TestBed.configureTestingModule({
            providers: [ThemeService]
        });
        service = TestBed.inject(ThemeService);
    });

    afterEach(() => {
        localStorage.clear();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should have isDark method', () => {
        expect(service.isDark).toBeDefined();
    });

    it('should have toggleTheme method', () => {
        expect(service.toggleTheme).toBeDefined();
    });

    it('should have isDark$ observable', () => {
        expect(service.isDark$).toBeDefined();
    });

    it('should return boolean from isDark', () => {
        expect(typeof service.isDark()).toBe('boolean');
    });
});
