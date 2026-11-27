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
package org.verapdf.gf.model.impl.cos;

import org.verapdf.cos.COSBase;
import org.verapdf.cos.COSInteger;
import org.verapdf.cos.COSNumber;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSReal;
import org.verapdf.model.coslayer.CosNumber;

/**
 * Common adapter for PDF integer and real numbers, exposing the numeric views used by profile rules.
 *
 * <p>The constructor caches both parser projections. {@link #fromPDFParserNumber(COSBase)} selects the concrete
 * integer or real wrapper from the parser's COS type and returns null for unsupported types; integer access is
 * truncated by the underlying parser's integer conversion.</p>
 *
 * <p><b>.NET porting notes:</b> Use {@code VeraPdf.Parser.Cos.CosInteger} and {@code CosReal} under
 * {@code CosValue}; implement the validation number contract with nullable {@code long?} and {@code double?}.
 * Preserve the source type dispatch and truncation behavior rather than rounding decimal values.</p>
 *
 * @author Timur Kamalov
 */
public class GFCosNumber extends GFCosObject implements CosNumber {

    private final long longVal;
    private final double doubleVal;

    protected GFCosNumber(COSNumber number, final String type) {
        super(number, type);
        this.longVal = number.getInteger();
        this.doubleVal = number.getReal();
    }

    public static GFCosNumber fromPDFParserNumber(COSBase number) {
        if (number.getType() == COSObjType.COS_INTEGER) {
            return new GFCosInteger((COSInteger) number);
        } else if (number.getType() == COSObjType.COS_REAL) {
            return new GFCosReal((COSReal) number);
        }
        return null;
    }

    /**
     * Get the string representing this object
     */
    @Override
    public String getstringValue() {
        return String.valueOf(this.doubleVal);
    }

    /**
     * Get truncated integer value
     */
    @Override
    public Long getintValue() {
        return this.longVal;
    }

    /**
     * Get original decimal value
     */
    @Override
    public Double getrealValue() {
        return this.doubleVal;
    }

}
