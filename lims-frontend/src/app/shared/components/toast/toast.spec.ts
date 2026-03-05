import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { Toast } from './toast';
import { ToastService } from '../../../core/services/toast';

describe('Toast', () => {
    let component: Toast;
    let fixture: ComponentFixture<Toast>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Toast],
            providers: [ToastService],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
        fixture = TestBed.createComponent(Toast);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have toastService injected', () => {
        expect(component.toastService).toBeTruthy();
    });

    it('should return bg-success for success type', () => {
        expect(component.getToastBgClass('success')).toBe('bg-success');
    });

    it('should return bg-error for error type', () => {
        expect(component.getToastBgClass('error')).toBe('bg-error');
    });

    it('should return bg-warning for warning type', () => {
        expect(component.getToastBgClass('warning')).toBe('bg-warning');
    });

    it('should return bg-primary for info type', () => {
        expect(component.getToastBgClass('info')).toBe('bg-primary');
    });

    it('should return default class for unknown type', () => {
        expect(component.getToastBgClass('unknown')).toBe('bg-slate-800');
    });
});
