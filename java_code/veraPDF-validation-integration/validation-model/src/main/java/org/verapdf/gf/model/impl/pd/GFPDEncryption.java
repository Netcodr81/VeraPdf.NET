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
package org.verapdf.gf.model.impl.pd;

import org.verapdf.model.pdlayer.PDEncryption;

/**
 * Validation-model adapter for the PDF encryption dictionary.
 *
 * <p>It exposes the numeric permissions/intersection value {@code P} used by PDF security
 * and encryption validation rules.  The Java parser still owns actual decryption and key access.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.GFPDEncryption}, implementing
 *       {@code VeraPdf.Model.PDLayer.IPDEncryption}.</li>
 *   <li>Map {@code P} to {@code long?}; the value is a signed PDF integer and must not be narrowed.</li>
 * </ul>
 *
 * @author Maxim Plushchov
 */
public class GFPDEncryption extends GFPDObject implements PDEncryption {

    public static final String ENCRYPTION_TYPE = "PDEncryption";

    public GFPDEncryption(org.verapdf.pd.encryption.PDEncryption encryptionDictionary) {
        super(encryptionDictionary, ENCRYPTION_TYPE);
    }

    @Override
    public Long getP() {
        return ((org.verapdf.pd.encryption.PDEncryption)this.simplePDObject).getP();
    }
}
