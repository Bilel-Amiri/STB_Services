import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InitierVirementComponent } from './initier-virement.component';

describe('InitierVirementComponent', () => {
  let component: InitierVirementComponent;
  let fixture: ComponentFixture<InitierVirementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ InitierVirementComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(InitierVirementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
