import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { StatusBadge } from './status-badge';

describe('StatusBadge', () => {
    let component: StatusBadge;
    let fixture: ComponentFixture<StatusBadge>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [StatusBadge],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
        fixture = TestBed.createComponent(StatusBadge);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default status to empty string', () => {
        expect(component.status).toBe('');
    });

    it('should return green class for active status', () => {
        component.status = 'Active';
        expect(component.getStatusClass()).toContain('green');
    });

    it('should return amber class for submitted status', () => {
        component.status = 'Submitted';
        expect(component.getStatusClass()).toContain('amber');
    });

    it('should return red class for rejected status', () => {
        component.status = 'Rejected';
        expect(component.getStatusClass()).toContain('red');
    });

    it('should return blue class for draft status', () => {
        component.status = 'Draft';
        expect(component.getStatusClass()).toContain('blue');
    });

    it('should handle case insensitivity', () => {
        component.status = 'ACTIVE';
        expect(component.getStatusClass()).toContain('green');
    });
});
