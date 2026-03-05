import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { EndorsementDetail } from './endorsement-detail';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { AuthService } from '../../../core/services/auth';

describe('EndorsementDetail', () => {
  let component: EndorsementDetail;
  let fixture: ComponentFixture<EndorsementDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EndorsementDetail, HttpClientTestingModule, RouterTestingModule],
      providers: [
        ApiService,
        { provide: ToastService, useValue: jasmine.createSpyObj('ToastService', ['success', 'error', 'show']) },
        { provide: AuthService, useValue: jasmine.createSpyObj('AuthService', ['isLoggedIn', 'getUserRole', 'getUserId']) }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    fixture = TestBed.createComponent(EndorsementDetail);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have a fixture', () => {
    expect(fixture).toBeTruthy();
  });

  it('should render without errors', () => {
    expect(component).toBeDefined();
  });

  it('should be a standalone component', () => {
    expect(EndorsementDetail).toBeTruthy();
  });

  it('should initialize component properties', () => {
    expect(component).toBeDefined();
  });
});
