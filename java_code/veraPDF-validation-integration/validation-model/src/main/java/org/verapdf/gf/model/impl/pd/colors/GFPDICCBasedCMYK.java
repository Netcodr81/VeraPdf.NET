/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd.colors;

import org.verapdf.model.pdlayer.PDICCBasedCMYK;

/**
 * Adds CMYK overprint state to an ICCBased color-space adapter.
 *
 * <p>This specialization associates an ICCBased CMYK space with the overprint mode
 * and overprinting flag captured from graphics state. The values affect whether zero
 * colorant samples preserve underlying separations and are exposed for PDF/A rendering
 * checks. ICC profile behavior remains in the parent adapter.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDICCBasedCMYK}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDICCBasedCMYK}. Keep the boxed Java fields nullable in
 * the model contract ({@code long?}, {@code bool?}) if the source semantics can be absent.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDICCBasedCMYK extends GFPDICCBased implements PDICCBasedCMYK {

    /** Model type name for an ICCBased CMYK space with overprint state. */
    public static final String ICC_BASED_CMYK_TYPE = "PDICCBasedCMYK";

    /** Overprint mode captured when this adapter is created. */
    private final Long opm;
    /** Overprinting flag captured when this adapter is created. */
    private final Boolean overprintingFlag;

    /** Creates the adapter with the graphics-state overprint values. */
    public GFPDICCBasedCMYK(org.verapdf.pd.colors.PDICCBased simplePDObject, int op, boolean overprintingFlag) {
        super(simplePDObject, ICC_BASED_CMYK_TYPE);
        this.opm = (long) op;
        this.overprintingFlag = overprintingFlag;
    }

    @Override
    /** Returns the captured overprint mode. */
    public Long getOPM() {
        return this.opm;
    }

    @Override
    /** Returns the captured overprinting flag. */
    public Boolean getoverprintFlag() {
        return this.overprintingFlag;
    }

    @Override
    /** Extends the parent identity so different overprint states are not conflated. */
    public String getID() {
        return super.getID() + " " + this.opm + " " + this.overprintingFlag;
    }
}
